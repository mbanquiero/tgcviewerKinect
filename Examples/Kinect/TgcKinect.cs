using System;
using Microsoft.Kinect;
using System.IO;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils;
using Examples.Expo;

namespace Examples.Kinect
{
    // kinect physicall interaction region
    public struct KPIR
    {
        public float x_min;
        public float x_max;
        public float y_min;
        public float y_max;
    }

    public class TgcKinect
    {

        public KinectSensor sensor;
        public KinectStatus lastStatus;
        public Skeleton[] auxSkeletonData;
        public bool sin_sensor;
        public Vector3 raw_pos_mano = new Vector3(0, 0, 0);
        public Vector3 raw_pos_cadera = new Vector3(0,0,0);
        public int skeleton_sel;
        public KPIR right_pir = new KPIR();          // right physicall interaction region
        public KPIR left_pir = new KPIR();           // left physicall interaction region

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

        Vector2 hands2dSpeed;
        /// <summary>
        /// Factor de velocidad de movimiento de las manos en 2D en x e y
        /// </summary>
        public Vector2 Hands2dSpeed
        {
            get { return hands2dSpeed; }
            set { hands2dSpeed = value; }
        }

        Vector2 cursorSize;
        /// <summary>
        /// Tamaño en pixels del cursor 2D que representa las manos en screen-space
        /// </summary>
        public Vector2 CursorSize
        {
            get { return cursorSize; }
            set { cursorSize = value; }
        }

        public bool skeletonTracked = false;
        public Vector3 skeletonCenter;
        public Vector3 sceneCenter;
        public float skeletonOffsetY = 0;



        /// <summary>
        /// Constructor
        /// </summary>
        public TgcKinect()
        {
            debugSkeleton = new TgcKinectDebugSkeleton();
            positionScale = 100;
            positionTranslate = new Vector3(0, 9, -25);
            data = new TgcKinectSkeletonData();
            historyFramesCount = 50;
            bodyProportion = 6;
            hands2dSpeed = new Vector2(1, 2);
            cursorSize = new Vector2(64, 64);
            skeletonCenter = new Vector3();
            // PIR
            //X: -0.05 a -0.2
            //Y: 0.3 a 0.5
            right_pir.x_min = -0.2f;
            right_pir.x_max = -0.05f;
            right_pir.y_min = 0.3f;
            right_pir.y_max = 0.5f;
            //Left
            //X: 0.05 a 0.2
            left_pir.x_min = 0.05f;
            left_pir.x_max = 0.2f;
            left_pir.y_min = 0.3f;
            left_pir.y_max = 0.5f;

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
            skeleton_sel = -1;

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

                    Skeleton rawSkeleton = null;
                    // Puede haber varios esqueletos, si es asi, tomo el que mas se parece al esqueleto anterior
                    Vector3 pos_ant = raw_pos_cadera;
                    float min_dist = float.MaxValue;

                    for (int i = 0; i < auxSkeletonData.Length; i++)
                    {
                        if (auxSkeletonData[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // Hay un esqueleto traqueado
                            SkeletonPoint pos_cadera = auxSkeletonData[i].Joints[JointType.HipCenter].Position;
                            Vector3 pos = new Vector3(pos_cadera.X,pos_cadera.Y,pos_cadera.Z);
                            float cur_dist = skeleton_sel != -1?
                                // Si ya habia otro tomo la distancia dicho esqueleto anterior
                                cur_dist = (pos - raw_pos_cadera).Length() : 
                                // si no habia ningun otro esqueleto trackeado, considero la dist. a la camara
                                cur_dist = pos_cadera.Z;

                            if (cur_dist < min_dist)
                            {
                                // Encontre un candidato a trackear
                                min_dist = cur_dist;
                                rawSkeleton = auxSkeletonData[skeleton_sel = i];
                                raw_pos_cadera = pos;
                            }
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
            if (!skeletonTracked)
            {
                if (rawSkeleton.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Tracked)
                {
                    skeletonTracked = true;
                    skeletonCenter = TgcKinectUtils.toVector3(rawSkeleton.Joints[JointType.HipCenter].Position);
                }
            }




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
            if (data.HandsFrames.Count > this.historyFramesCount)
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




        public float halfBodyWidth = float.MinValue;
        public float halfBodyHeight = float.MinValue;


        /// <summary>
        /// Actualizar posicion 2D de las manos
        /// </summary>
        private void updateHandsScreenPos(Skeleton rawSkeleton, TgcKinectSkeletonData data)
        {
            /*
            //Calcular boundingBox 2D para la mano derecha: entre la cabeza, el hombro derecho y la spine.
            data.Current.RScreenRect = TgcKinectUtils.computeScreenRect(new SkeletonPoint[]{
                rawSkeleton.Joints[JointType.ShoulderRight].Position,
                rawSkeleton.Joints[JointType.Head].Position,
                rawSkeleton.Joints[JointType.Spine].Position,
            });

            //Calcular boundingBox 2D para la mano izquierda: entre la cabeza, el hombro izq y la spine.
            data.Current.LScreenRect = TgcKinectUtils.computeScreenRect(new SkeletonPoint[]{
                rawSkeleton.Joints[JointType.ShoulderLeft].Position,
                rawSkeleton.Joints[JointType.Head].Position,
                rawSkeleton.Joints[JointType.Spine].Position,
            });


            //Agrandar ambos boundingBox hacia el lado donde esta la mano
            data.Current.RScreenRect = new RectangleF(data.Current.RScreenRect.Location, new SizeF(data.Current.RScreenRect.Width + data.Current.RScreenRect.Width / 2, data.Current.RScreenRect.Height));
            data.Current.LScreenRect = new RectangleF(new PointF(data.Current.LScreenRect.X - data.Current.LScreenRect.Width / 2, data.Current.LScreenRect.Y), data.Current.LScreenRect.Size);
            
            


            //Clampear posicion 2D de manos segun el boundingBox 2D
            Vector2 rHand2dPos = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HandRight].Position);
            Vector2 lHand2dPos = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HandLeft].Position);
            rHand2dPos = TgcKinectUtils.clampToRect(rHand2dPos, data.Current.RScreenRect);
            lHand2dPos = TgcKinectUtils.clampToRect(lHand2dPos, data.Current.LScreenRect);

            //Mapear puntos al tamaño de la pantalla para obtener posicion 2D de las manos
            Viewport screenViewport = GuiController.Instance.D3dDevice.Viewport;
            data.Current.RightHandPos = TgcKinectUtils.mapPointToScreen(rHand2dPos, data.Current.RScreenRect, screenViewport, cursorSize);
            data.Current.LefttHandPos = TgcKinectUtils.mapPointToScreen(lHand2dPos, data.Current.LScreenRect, screenViewport, cursorSize);

            //Distancia z relativa de cada mano al hombro
            data.Current.RightZDist = rawSkeleton.Joints[JointType.HandRight].Position.Z - rawSkeleton.Joints[JointType.ShoulderRight].Position.Z;
            data.Current.LeftZDist = rawSkeleton.Joints[JointType.HandLeft].Position.Z - rawSkeleton.Joints[JointType.ShoulderLeft].Position.Z;
            */


            /*
            Vector2 hipRight = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HipRight].Position);
            Vector2 hipLeft = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HipLeft].Position);
            Vector2 spine = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.Spine].Position);
            Vector2 head = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.Head].Position);

            RectangleF bounds = new RectangleF();
            bounds.X = hipLeft.X;
            bounds.Width = hipRight.X - hipLeft.X;

            float aspectRatio = TgcD3dDevice.aspectRatio;
            bounds.Y = spine.Y + (head.Y - spine.Y) / 4;
            bounds.Height = (bounds.Width / aspectRatio) * 2.5f;



            RectangleF rScreenRect = new RectangleF();
            rScreenRect.X = bounds.X + bounds.Width * 0.25f;
            rScreenRect.Width = bounds.Width;
            rScreenRect.Y = bounds.Y;
            rScreenRect.Height = bounds.Height;

            RectangleF lScreenRect = new RectangleF();
            lScreenRect.X = bounds.X + bounds.Width * 0.25f;
            lScreenRect.Width = bounds.Width;
            lScreenRect.Y = bounds.Y;
            lScreenRect.Height = bounds.Height;


            //Multiplicar posicion 2D de las manos por factor de velocidad
            Vector2 rHand2dPos = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HandRight].Position);
            Vector2 lHand2dPos = TgcKinectUtils.toVector2(rawSkeleton.Joints[JointType.HandLeft].Position);

            //Clampear posicion 2D de manos segun el boundingBox 2D
            rHand2dPos = TgcKinectUtils.clampToRect(rHand2dPos, rScreenRect);
            lHand2dPos = TgcKinectUtils.clampToRect(lHand2dPos, lScreenRect);

            //Mapear puntos al tamaño de la pantalla para obtener posicion 2D de las manos
            Viewport screenViewport = GuiController.Instance.D3dDevice.Viewport;
            data.Current.RightHandPos = TgcKinectUtils.mapPointToScreen(rHand2dPos, rScreenRect, screenViewport, cursorSize);
            data.Current.LefttHandPos = TgcKinectUtils.mapPointToScreen(lHand2dPos, lScreenRect, screenViewport, cursorSize);
            */

            if (/*halfBodyWidth == float.MinValue &&*/
                rawSkeleton.Joints[JointType.ShoulderRight].TrackingState == JointTrackingState.Tracked &&
                rawSkeleton.Joints[JointType.Spine].TrackingState == JointTrackingState.Tracked &&
                rawSkeleton.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked)
            {
                halfBodyWidth = rawSkeleton.Joints[JointType.ShoulderRight].Position.X - rawSkeleton.Joints[JointType.Spine].Position.X;
                halfBodyHeight = rawSkeleton.Joints[JointType.Head].Position.Y - rawSkeleton.Joints[JointType.Spine].Position.Y;
            }


            if (rawSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked &&
                rawSkeleton.Joints[JointType.Head].TrackingState != JointTrackingState.NotTracked)
            {
                //Right
                // Actualizo esta posicion tal como viene de la kinect, para debuggear
                raw_pos_mano = new Vector3(rawSkeleton.Joints[JointType.HandRight].Position.X - rawSkeleton.Joints[JointType.Head].Position.X,
                                            rawSkeleton.Joints[JointType.HandRight].Position.Y - rawSkeleton.Joints[JointType.Head].Position.Y,
                                            rawSkeleton.Joints[JointType.HandRight].Position.Z - rawSkeleton.Joints[JointType.Head].Position.Z);
                //X: -0.05 a -0.2
                //Y: 0.1 a 0.5
                data.Current.RightHandPos = TgcKinectUtils.computeHand2DPos(
                    rawSkeleton.Joints[JointType.HandRight].Position,
                    rawSkeleton.Joints[JointType.Head].Position, right_pir, true);
            }

            if (rawSkeleton.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked &&
                rawSkeleton.Joints[JointType.Head].TrackingState != JointTrackingState.NotTracked)
            {
                //Left
                //X: 0.05 a 0.2
                //Y: 0.1 a 0.5
                data.Current.LefttHandPos = TgcKinectUtils.computeHand2DPos(
                    rawSkeleton.Joints[JointType.HandLeft].Position,
                    rawSkeleton.Joints[JointType.Head].Position, left_pir, false);
            }

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

            /*
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
            */
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

                    // Smoothed with some latency.
                    // Filters out medium jitters.
                    // Good for a menu system that needs to be smooth but
                    // doesn't need the reduced latency as much as gesture recognition does.
                    TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                    {
                        smoothingParam.Smoothing = 0.5f;
                        smoothingParam.Correction = 0.1f;
                        smoothingParam.Prediction = 0.5f;
                        smoothingParam.JitterRadius = 0.1f;
                        smoothingParam.MaxDeviationRadius = 0.1f;
                    };

                    this.sensor.SkeletonStream.Enable(smoothingParam);
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
            /*
            SkeletonPoint p2 = new SkeletonPoint();
            p2.X = p.X * positionScale * -1f + positionTranslate.X;
            p2.Y = p.Y * positionScale + positionTranslate.Y;
            p2.Z = p.Z * positionScale + positionTranslate.Z;
            return p2;
             */

            SkeletonPoint p2 = new SkeletonPoint();
            float offsetY = skeletonOffsetY * positionScale;
            p2.X = (p.X - skeletonCenter.X) * positionScale + sceneCenter.X;
            p2.Y = (p.Y - skeletonCenter.Y) * positionScale + sceneCenter.Y + offsetY;
            p2.Z = ((p.Z - skeletonCenter.Z) * -1) * positionScale + sceneCenter.Z;
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
