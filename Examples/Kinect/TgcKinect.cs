using System;
using Microsoft.Kinect;
using System.IO;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace Examples.Kinect
{
    public class TgcKinect
    {

        KinectSensor sensor;
        KinectStatus lastStatus;
        Skeleton[] auxSkeletonData;
        bool sin_sensor;

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

        Vector3 positionTranslate;
        /// <summary>
        /// Valor por el cual se trasladan las posiciones de los huesos luego de escaladas
        /// </summary>
        public Vector3 PositionTranslate
        {
            get { return positionTranslate; }
            set { positionTranslate = value; }
        }

        int historyFramesCount;
        /// <summary>
        /// Tamaño del buffer de historial de frames
        /// </summary>
        public int HistoryFramesCount
        {
            get { return historyFramesCount; }
            set { historyFramesCount = value; }
        }

        float bodyProportion;
        /// <summary>
        /// Es la proporcion del esqueleto generico que se usa.
        /// Se define como la distancia entre la cabeza (Head) y la cintura (HipCenter).
        /// El esqueleto es reajustado para mantener esta proporcion.
        /// </summary>
        public float BodyProportion
        {
            get { return bodyProportion; }
            set { bodyProportion = value; }
        }

        float hands2dSpeed;
        /// <summary>
        /// Factor de velocidad de movimiento de las manos en 2D
        /// </summary>
        public float Hands2dSpeed
        {
            get { return hands2dSpeed; }
            set { hands2dSpeed = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TgcKinect()
        {
            debugSkeleton = new TgcKinectDebugSkeleton();
            positionScale = 10;
            positionTranslate = new Vector3(0, 9, -25);
            data = new TgcKinectSkeletonData();
            historyFramesCount = 50;
            bodyProportion = 6;
            hands2dSpeed = 1;
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
                this.data.Active = true;

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

            //Copiar posicion central
            data.Previous.CenterPos = data.Current.CenterPos;

            //Copiar BSphere de frame actual a frame anterior
            data.Previous.RightHandSphere.setCenter(data.Current.RightHandSphere.Center);
            data.Previous.LeftHandSphere.setCenter(data.Current.LeftHandSphere.Center);

            //Copiar pos2D de las dos manos al frame anterior
            data.Previous.RightHandPos = data.Current.RightHandPos;
            data.Previous.LefttHandPos = data.Current.LefttHandPos;

            



            //Copiar esqueleto recien trackeado al frame actual, adaptando proporciones
            this.copySkeleton(rawSkeleton, data.Current.KinectSkeleton, true);

            //Actualizar posicion central
            data.Current.CenterPos = TgcKinectUtils.toVector3(data.Current.KinectSkeleton.Joints[JointType.HipCenter].Position);

            //Actualizar BSphere de manos de frame actual
            data.Current.RightHandSphere.setCenter(TgcKinectUtils.toVector3(data.Current.KinectSkeleton.Joints[JointType.HandRight].Position));
            data.Current.LeftHandSphere.setCenter(TgcKinectUtils.toVector3(data.Current.KinectSkeleton.Joints[JointType.HandLeft].Position));

            //Actualizar posicion 2D de manos
            this.updateHandsScreenPos(rawSkeleton, data);


            //Agregar nuevo cuadro a historial
            TgcKinectSkeletonData.HandFrame newFrame = new TgcKinectSkeletonData.HandFrame();
            newFrame.Pos3D = new Vector3[2];
            newFrame.Pos2D = new Vector2[2];
            newFrame.Pos3D[TgcKinectSkeletonData.RIGHT_HAND] = data.Current.RightHandSphere.Center;
            newFrame.Pos3D[TgcKinectSkeletonData.LEFT_HAND] = data.Current.LeftHandSphere.Center;
            newFrame.Pos2D[TgcKinectSkeletonData.RIGHT_HAND] = data.Current.RightHandPos;
            newFrame.Pos2D[TgcKinectSkeletonData.LEFT_HAND] = data.Current.LefttHandPos;
            data.HandsFrames.AddFirst(newFrame);

            //Ver si hay que eliminar el ultimo cuadro viejo del historial
            if(data.HandsFrames.Count > this.historyFramesCount)
            {
                data.HandsFrames.RemoveLast();
            }


            //Hacer analisis de datos en el historial de frames, para la mano derecha
            this.computeAxisAnalysis(data, TgcKinectSkeletonData.RIGHT_HAND, 0);
            this.computeAxisAnalysis(data, TgcKinectSkeletonData.RIGHT_HAND, 1);
            this.computeAxisAnalysis(data, TgcKinectSkeletonData.RIGHT_HAND, 2);

            //Hacer analisis de datos en el historial de frames, para la mano izquierda
            this.computeAxisAnalysis(data, TgcKinectSkeletonData.LEFT_HAND, 0);
            this.computeAxisAnalysis(data, TgcKinectSkeletonData.LEFT_HAND, 1);
            this.computeAxisAnalysis(data, TgcKinectSkeletonData.LEFT_HAND, 2);

        }

        /// <summary>
        /// Actualizar posicion 2D de las manos
        /// </summary>
        private void updateHandsScreenPos(Skeleton rawSkeleton, TgcKinectSkeletonData data)
        {
            //Calcular boundingBox 2D entre la cabeza, los hombres y la spine (en base al rawSkeleton)
            RectangleF bodyScreenRect = TgcKinectUtils.computeScreenRect(new SkeletonPoint[]{
                rawSkeleton.Joints[JointType.ShoulderLeft].Position,
                rawSkeleton.Joints[JointType.ShoulderRight].Position,
                rawSkeleton.Joints[JointType.Head].Position,
                rawSkeleton.Joints[JointType.Spine].Position,
            });

            //Agrandar un cuarto de ancho de cada lado para tener mas lugar para las manos
            float bodyScreenHalfWidth = bodyScreenRect.Width / 2;
            bodyScreenRect.X -= bodyScreenHalfWidth;
            bodyScreenRect.Width += bodyScreenHalfWidth;

            //Multiplicar posicion 2D de las manos por factor de velocidad
            Vector2 rHand2dPos = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HandRight].Position) * hands2dSpeed;
            Vector2 lHand2dPos = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HandLeft].Position) * hands2dSpeed;

            //Clampear posicion 2D de manos segun el boundingBox 2D
            rHand2dPos = TgcKinectUtils.clampToRect(rHand2dPos, bodyScreenRect);
            lHand2dPos = TgcKinectUtils.clampToRect(lHand2dPos, bodyScreenRect);

            //Mapear puntos al tamaño de la pantalla para obtener posicion 2D de las manos
            Viewport screenViewport = GuiController.Instance.D3dDevice.Viewport;
            data.Current.RightHandPos = TgcKinectUtils.mapPointToScreen(rHand2dPos, bodyScreenRect, screenViewport);
            data.Current.LefttHandPos = TgcKinectUtils.mapPointToScreen(lHand2dPos, bodyScreenRect, screenViewport);

            /* Forma MARIAN
            SkeletonPoint p1 = rawSkeleton.Joints[JointType.HandRight].Position;
            SkeletonPoint p2 = rawSkeleton.Joints[JointType.Head].Position;
            data.Current.RightHandPos = new Vector2(
                (((p1.X - p2.X) + 0.12f) / 0.37f) * GuiController.Instance.D3dDevice.Viewport.Width,
                (1 - (((p1.Y - p2.Y) + 0.3f) / 0.36f)) * GuiController.Instance.D3dDevice.Viewport.Height);

            p1 = rawSkeleton.Joints[JointType.HandLeft].Position;
            data.Current.LefttHandPos = new Vector2(p1.X - p2.X, p1.Y - p2.Y);
            */
        }

        /// <summary>
        /// Hacer analisis estadistico de los datos de posicion de una mano del esqueleto, en un eje determinado.
        /// Guarda los datos en el AxisAnalysisData de ese eje, para esa mano
        /// </summary>
        private void computeAxisAnalysis(TgcKinectSkeletonData data, int handIndex, int axisIndex)
        {
            //Lugar donde tenemos que almacenar el resultado
            TgcKinectSkeletonData.AxisAnalysisData analysis = data.HandsAnalysisData[handIndex][axisIndex];

            int framesCount = data.HandsFrames.Count;
            analysis.Min = float.MaxValue;
            analysis.Max = float.MinValue;
            float sum = 0;
            int i = 0;
            float value = 0;
            float lastValue = 0;
            float sumDiff = 0;
            foreach (TgcKinectSkeletonData.HandFrame frame in data.HandsFrames)
            {
                lastValue = value;
                value = frame.get3DValue(handIndex, axisIndex);
                sum += value;

                //min
                if (value < analysis.Min)
                {
                    analysis.Min = value;
                }
                //max
                if (value > analysis.Max)
                {
                    analysis.Max = value;
                }

                //diff con el anterior
                if (i > 0)
                {
                    sumDiff += value - lastValue;
                }


                i++;
            }

            //avg
            analysis.Avg = sum / framesCount;

            //diff
            analysis.DiffAvg = sumDiff / (framesCount - 1);

            //variance
            float sumVariance = 0;
            foreach (TgcKinectSkeletonData.HandFrame frame in data.HandsFrames)
            {
                value = frame.get3DValue(handIndex, axisIndex);
                sumVariance += FastMath.Abs(analysis.Avg - value);
            }
            analysis.Variance = sumVariance / framesCount;
        }





        /// <summary>
        /// CopySkeleton copies the data from another skeleton.
        /// Escala las posiciones
        /// </summary>
        /// <param name="source">The source skeleton.</param>
        /// <param name="destination">The destination skeleton.</param>
        /// <param name="scalePos">Indica si hay que adaptar las posiciones del esqueleto.</param>
        private void copySkeleton(Skeleton source, Skeleton destination, bool scalePos)
        {
            destination.TrackingState = source.TrackingState;
            destination.TrackingId = source.TrackingId;
            destination.ClippedEdges = source.ClippedEdges;

            //Escalar posicion
            destination.Position = scalePos ? getScaledTranslatedPoint(source.Position) : source.Position;
            
            // This must copy before the joint orientations
            Array jointTypeValues = Enum.GetValues(typeof(JointType));
            foreach (JointType j in jointTypeValues)
            {
                Joint temp = destination.Joints[j];
                temp.Position = scalePos ? getScaledTranslatedPoint(source.Joints[j].Position) : source.Joints[j].Position;
                temp.TrackingState = source.Joints[j].TrackingState;
                destination.Joints[j] = temp;
            }

            /* No se está usando
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
             */ 

            //Ajuste de proporciones
            if (scalePos)
            {
                //Obtener la proporcion del esqueleto, medida como la distancia entre la cabeza y la cintura
                Vector3 headPos = TgcKinectUtils.toVector3(destination.Joints[JointType.Head].Position);
                Vector3 centerPos = TgcKinectUtils.toVector3(destination.Joints[JointType.HipCenter].Position);
                float currentProportion = Vector3.Length(headPos - centerPos);
                float formFactor = bodyProportion / currentProportion;

                //Ajustar todas las posiciones de los huesos al formFactor
                destination.Position = TgcKinectUtils.mul(destination.Position, formFactor);
                foreach (JointType j in jointTypeValues)
                {
                    Joint temp = destination.Joints[j];
                    temp.Position = TgcKinectUtils.mul(destination.Joints[j].Position, formFactor);
                    destination.Joints[j] = temp;
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
                sin_sensor = true;
                return;
                //throw new Exception("No KinectSensors detected");
            }
            sin_sensor = false;
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
                     this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
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
        public SkeletonPoint getScaledTranslatedPoint(SkeletonPoint p)
        {
            SkeletonPoint p2 = new SkeletonPoint();
            p2.X = p.X * positionScale * -1f + positionTranslate.X;
            p2.Y = p.Y * positionScale + positionTranslate.Y;
            p2.Z = p.Z * positionScale + positionTranslate.Z;
            return p2;
        }


        public void dispose()
        {
            sensor.Dispose();
            sensor = null;
            debugSkeleton.dispose();
        }

    }
}
