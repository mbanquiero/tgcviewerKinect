using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils._2D;
using System.Drawing;
using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.Kinect;
using Examples.Kinect;

namespace Examples.Expo
{
    /// <summary>
    /// Utilidades para logging grande
    /// </summary>
    public class BigLogger
    {
        private static BigLogger instance;
        /// <summary>
        /// Singleton
        /// </summary>
        public static BigLogger Instance
        {
            get {
                if (instance == null) instance = new BigLogger();
                return BigLogger.instance; 
            }
        }


        TgcText2d logger;

        private BigLogger()
        {
            logger = new TgcText2d();
            logger.Align = TgcText2d.TextAlign.LEFT;
            logger.Color = Color.Black;
            logger.changeFont(new System.Drawing.Font("verdana", 24, System.Drawing.FontStyle.Bold));
            logger.Size = new Size(GuiController.Instance.D3dDevice.Viewport.Width -20, GuiController.Instance.D3dDevice.Viewport.Height -20);
            logger.Position = new Point(20, 20);
            logger.Text = "";
        }
        public static void clearLog()
        {
            BigLogger.Instance.logger.Text = "";
        }

        public static void log(string text)
        {
            BigLogger.Instance.logger.Text += text + Environment.NewLine;
        }

        public static void log(string text, Vector3 v)
        {
            BigLogger.log(text + ": " + TgcParserUtils.printVector3(v));
        }

        public static void log(string text, Vector2 v)
        {
            BigLogger.log(text + ": " + TgcParserUtils.printVector2(v));
        }

        public static void log(string text, float n)
        {
            BigLogger.log(text + ": " + TgcParserUtils.printFloat(n));
        }

        public static void log(string text, SkeletonPoint p)
        {
            BigLogger.log(text, TgcKinectUtils.toVector3(p));
        }

        public static void renderLog()
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            BigLogger.Instance.logger.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
            BigLogger.clearLog();
        }
        


    }
}
