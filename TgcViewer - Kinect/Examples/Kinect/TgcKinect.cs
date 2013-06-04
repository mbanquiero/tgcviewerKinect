using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.IO;
using Microsoft.DirectX;

namespace Examples.Kinect
{
    public class TgcKinect
    {

        KinectSensor sensor;
        KinectStatus lastStatus;
        Skeleton[] auxSkeletonData;

        TgcKinectSkeletonData data;
        /// <summary>
        /// Datos del esqueleto trackeado
        /// </summary>
        public TgcKinectSkeletonData Data
        {
            get { return data; }
        }

        TgcKinectDebugSkeleton debugSkeleton;
        /// <summary>
        /// Herramienta para dibujar esqueleto en modo debug
        /// </summary>
        public TgcKinectDebugSkeleton DebugSkeleton
        {
            get { return debugSkeleton; }
        }

        float positionScale;
        /// <summary>
        /// Valor por el cual se escalan las posiciones de los huesos
        /// </summary>
        public float PositionScale
        {
            get { return positionScale; }
            set { positionScale = value; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public TgcKinect()
        {
            debugSkeleton = new TgcKinectDebugSkeleton();
            positionScale = 100;
            data = new TgcKinectSkeletonData();
        }

        /// <summary>
        /// Iniciar Kinect
        /// </summary>
        public void init()
        {
            KinectSensor.KinectSensors.StatusChanged += this.kinectSensors_StatusChanged;
            this.discoverSensor();
        }


        /// <summary>
        /// Obtener datos de esqueleto. Si no encuentra datos validos devuelve null.
        /// </summary>
        /// <returns>Esqueleto capturado o null si no pudo capturar</returns>
        public TgcKinectSkeletonData update()
        {
            this.data.Active = false;

            // If the sensor is not found, not running, or not connected, stop now
            if (null == this.sensor || false == this.sensor.IsRunning || this.sensor.Status != KinectStatus.Connected)
            {
                return this.data;
            }

            using (var skeletonFrame = this.sensor.SkeletonStream.OpenNextFrame(0))
            {
                // Sometimes we get a null frame back if no data is ready
                if (null != skeletonFrame)
                {
                    // Reallocate if necessary
                    if (null == auxSkeletonData || auxSkeletonData.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        auxSkeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(auxSkeletonData);

                    // Select the first tracked skeleton we see to avateer
                    Skeleton rawSkeleton = null;
                    for (int i = 0; i < auxSkeletonData.Length; i++)
                    {
                        if (auxSkeletonData[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            rawSkeleton = auxSkeletonData[i];
                            break;
                        }
                    }
                    if (rawSkeleton == null)
                    {
                        return this.data;
                    }

                    //Capturar datos del esqueleto
                    this.data.Active = true;
                    this.buildSkeletonData(rawSkeleton, this.data);
                }
            }


            return this.data;
        }

        /// <summary>
        /// Actualizar informacion de esqueleto
        /// </summary>
        /// <param name="rawSkeleton">Datos crudos trackeados</param>
        /// <param name="data">Datos procesados que se actualizan</param>
        private void buildSkeletonData(Skeleton rawSkeleton, TgcKinectSkeletonData data)
        {

            //Copiar esqueleto de frame actual a frame anterior, sin escalar las posiciones porque ya estaban escaladas de antes
            this.copySkeleton(data.Current.KinectSkeleton, data.Previous.KinectSkeleton, false);

            //Copiar BSphere de frame actual a frame anterior
            data.Previous.RightHandSphere.setCenter(data.Current.RightHandSphere.Center);
            data.Previous.LeftHandSphere.setCenter(data.Current.LeftHandSphere.Center);

            //Copiar esqueleto recien trackeado al frame actual, escalando posiciones
            this.copySkeleton(rawSkeleton, data.Current.KinectSkeleton, true);

            //Actualizar BSphere de manos de frame actual
            data.Current.RightHandSphere.setCenter(TgcKinect.toVector3(data.Current.KinectSkeleton.Joints[JointType.HandRight].Position));
            data.Current.LeftHandSphere.setCenter(TgcKinect.toVector3(data.Current.KinectSkeleton.Joints[JointType.HandLeft].Position));
        }







        /// <summary>
        /// CopySkeleton copies the data from another skeleton.
        /// Escala las posiciones
        /// </summary>
        /// <param name="source">The source skeleton.</param>
        /// <param name="destination">The destination skeleton.</param>
        /// <param name="scalePos">Indica si hay que escalar las posiciones.</param>
        private void copySkeleton(Skeleton source, Skeleton destination, bool scalePos)
        {
            destination.TrackingState = source.TrackingState;
            destination.TrackingId = source.TrackingId;
            destination.ClippedEdges = source.ClippedEdges;

            //Escalar posicion
            destination.Position = getScaledPoint(source.Position);
            
            Array jointTypeValues = Enum.GetValues(typeof(JointType));

            // This must copy before the joint orientations
            foreach (JointType j in jointTypeValues)
            {
                Joint temp = destination.Joints[j];
                temp.Position = getScaledPoint(source.Joints[j].Position);
                temp.TrackingState = source.Joints[j].TrackingState;
                destination.Joints[j] = temp;
            }

            if (null != source.BoneOrientations)
            {
                foreach (JointType j in jointTypeValues)
                {
                    BoneOrientation temp = destination.BoneOrientations[j];
                    temp.HierarchicalRotation.Matrix = source.BoneOrientations[j].HierarchicalRotation.Matrix;
                    temp.HierarchicalRotation.Quaternion = source.BoneOrientations[j].HierarchicalRotation.Quaternion;
                    temp.AbsoluteRotation.Matrix = source.BoneOrientations[j].AbsoluteRotation.Matrix;
                    temp.AbsoluteRotation.Quaternion = source.BoneOrientations[j].AbsoluteRotation.Quaternion;
                    destination.BoneOrientations[j] = temp;
                }
            }
        }


        /// <summary>
        /// This wires up the status changed event to monitor for 
        /// Kinect state changes.  It automatically stops the sensor
        /// if the device is no longer available.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void kinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // If the status is not connected, try to stop it
            if (e.Status != KinectStatus.Connected)
            {
                e.Sensor.Stop();
            }

            this.lastStatus = e.Status;
            this.discoverSensor();
        }

        /// <summary>
        /// This method will use basic logic to try to grab a sensor.
        /// Once a sensor is found, it will start the sensor with the
        /// requested options.
        /// </summary>
        private void discoverSensor()
        {
            // Grab any available sensor
            if (KinectSensor.KinectSensors.Count == 0)
            {
                throw new Exception("No KinectSensors detected");
            }
            this.sensor = KinectSensor.KinectSensors[0];

            if (null != this.sensor)
            {
                this.lastStatus = this.sensor.Status;

                // If this sensor is connected, then enable it
                if (this.lastStatus == KinectStatus.Connected)
                {
                    // For many applications we would enable the
                    // automatic joint smoothing, however, in this
                    // Avateering sample, we perform skeleton joint
                    // position corrections, so we will manually
                    // filter when these are complete.

                    // Typical smoothing parameters for the joints:
                     var parameters = new TransformSmoothParameters
                     {
                         Smoothing = 0.25f,
                        Correction = 0.25f,
                        Prediction = 0.75f,
                        JitterRadius = 0.1f,
                        MaxDeviationRadius = 0.04f 
                     };
                     this.sensor.SkeletonStream.Enable(parameters);
                     //this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                     //this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                     this.sensor.SkeletonStream.EnableTrackingInNearRange = true; // Enable skeleton tracking in near mode
                     //this.sensor.DepthStream.Range = DepthRange.Near;

                    try
                    {
                        this.sensor.Start();
                    }
                    catch (IOException)
                    {
                        // sensor is in use by another application
                        // will treat as disconnected for display purposes
                        this.sensor = null;
                    }
                }
            }
            else
            {
                this.lastStatus = KinectStatus.Disconnected;
            }
        }

        /// <summary>
        /// Devuelve una posicion del esqueleto escalada por positionScale
        /// </summary>
        public SkeletonPoint getScaledPoint(SkeletonPoint p)
        {
            SkeletonPoint p2 = new SkeletonPoint();
            p2.X = p.X * positionScale * -1f;
            p2.Y = p.Y * positionScale;
            p2.Z = p.Z * positionScale;
            return p2;
        }

        /// <summary>
        /// Convierte de un SkeletonPoint a un Vector3
        /// </summary>
        public static Vector3 toVector3(SkeletonPoint p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }

        public void dispose()
        {
            sensor.Dispose();
            sensor = null;
            debugSkeleton.dispose();
        }

    }
}
