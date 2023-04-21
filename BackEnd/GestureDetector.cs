//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DiscreteGestureBasics
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;

    /// <summary>
    /// Gesture Detector class which listens for VisualGestureBuilderFrame events from the service
    /// and updates the associated GestureResultView object with the latest results for the 'Seated' gesture
    /// </summary>
    public class GestureDetector : IDisposable
    {
        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = @"Database\Seated.gbd";

        /// <summary> Name of the discrete gesture in the database that we want to track </summary>
        private readonly string punchGestureName = "PunchStart";

        private readonly string kickGestureName = "KickStart";

        private readonly string hadukenGestureName = "HadukStart";

        private readonly string jumpingJacksGestureName = "JumpingJacks";

        private readonly string punchProgressGestureName = "PunchProgress";

        private readonly string kickProgressGestureName = "KickProgress";

        private readonly string hadukProgressGestureName = "HadukProgress";

        bool punchStarted = false;
        bool kickStarted = false;
        bool hadukStarted = false;
        bool jumpingJacksStarted = false;
        float punchProgress = 0.0f;
        float kickProgress = 0.0f;
        float hadukProgress = 0.0f;
        float punchConfidence= 0.0f;
        float kickConfidence = 0.0f;
        float hadukConfidence= 0.0f;
        float jumpingJacksConfidence= 0.0f;


        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>KKKKKKK
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }
            BodyFrameReader bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            
            this.GestureResultView = gestureResultView;
            
            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            if (bodyFrameReader != null)
            {
                bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            }
            // load the 'Seated' gesture from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase))
            {
                // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
                // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
                foreach (Gesture gesture in database.AvailableGestures)
                {
                   
                        this.vgbFrameSource.AddGesture(gesture);
                    
                }
            }
        }
        

// Event handler for receiving skeletal tracking data
        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    Body[] bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies);

                    // Retrieve the lean angle for each tracked person
                    foreach (Body body in bodies)
                    {
                        if (body.IsTracked)
                        {
                            Joint spineBaseJoint = body.Joints[JointType.SpineBase];
                            Joint spineMidJoint = body.Joints[JointType.SpineMid];
                            double leanAngle = Math.Atan2(spineMidJoint.Position.Z - spineBaseJoint.Position.Z, spineMidJoint.Position.X - spineBaseJoint.Position.X) * 180.0 / Math.PI;
                            
                            leanAngle = Math.Abs(leanAngle);
                            //Console.WriteLine("Lean angle: " + leanAngle);
                            if (leanAngle > 160)
                            {
                                //Console.WriteLine("<<<<<<<<<<------------");
                                // Hold down the "A" key for 1 second
                                // Press the "A" key
                                // Hold down the "A" key for one second`
                                // Hold down the "A" key for one second
                                // Hold down the "A" key for one second
                                //SendKeys.SendWait("{A 10}");
                                //System.Threading.Thread.Sleep(100);
                               // SendKeys.SendWait("{UP}");

                            }
                            if (leanAngle < 20)
                            {
                                //Console.WriteLine("--------->>>>>>>>>>>>>>");
                                // Press the "A" key
                                // Hold down the "A" key for one second
                                // Hold down the "A" key for one second
                                // Hold down the "A" key for one second
                               // SendKeys.SendWait("{D 10}");
                                //System.Threading.Thread.Sleep(100);
                                //SendKeys.SendWait("{UP}"); ;
                            }
                        }
                    }
                }
            }
        }
        /// <summary> Gets the GestureResultView object which stores the detector results for display in the UI </summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        /// <summary>
        /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    // get the discrete gesture results which arrived with the latest frame
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                    IReadOnlyDictionary<Gesture, ContinuousGestureResult> continuousResults = frame.ContinuousGestureResults;

                    if (discreteResults != null)
                    {
                        //Console.WriteLine("Discrete Gesture Detected!");
                        /*bool punchStarted = this.GestureResultView.PunchStarted;
                        bool kickStarted = this.GestureResultView.KickStarted;
                        bool hadukStarted = this.GestureResultView.HadukStarted;
                        float punchProgress = this.GestureResultView.PunchProgress;
                        float kickProgress = this.GestureResultView.KickProgress;
                        float hadukProgress = this.GestureResultView.HadukProgress;*/

                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            //Console.WriteLine("Current Gesture: " + gesture.Name);

                            if(gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if(result != null)
                                {
                                    if(gesture.Name.Equals(this.punchGestureName)){
                                        //Console.WriteLine("Discrete Punch");
                                        if (result.Confidence >= 0.8)
                                        {
                                            punchStarted = result.Detected;
                                        }
                                            punchConfidence = result.Confidence;
                                    }
                                    else if (gesture.Name.Equals(this.kickGestureName) && result.Confidence >= 0.5){
                                        // Console.WriteLine("Discrete Kick");
                                            kickStarted = result.Detected;
                                            kickConfidence = result.Confidence;
                                        
                                        
                                    }
                                    else if (gesture.Name.Equals(this.hadukenGestureName))
                                    {
                                        //Console.WriteLine("Discrete Haduken");

                                        if (result.Confidence >= 0.8)
                                        {
                                            hadukStarted = result.Detected;
                                        }
                                            hadukConfidence = result.Confidence;
                                       
                                    }
                                    else if (gesture.Name.Equals(this.jumpingJacksGestureName) && result.Confidence >= 0.8)
                                    {
                                            jumpingJacksStarted = result.Detected;
                                            jumpingJacksConfidence = result.Confidence;
                                    }
                                }
                            }
                            /*
                            if (continuousResults != null)
                            {
                                //Console.WriteLine("Continuous Gesture Detected");
                                if (gesture.GestureType == GestureType.Continuous)
                                {
                                    ContinuousGestureResult result = null;
                                    continuousResults.TryGetValue(gesture, out result);
                                    if(result != null)
                                    {
                                        
                                        if (gesture.Name.Equals(this.punchProgressGestureName)){
                                            //Console.WriteLine("                  Continuous punch");
                                            punchProgress = result.Progress;
                                        }
                                        if (gesture.Name.Equals(this.kickProgressGestureName)){
                                            //Console.WriteLine("                          Continuous kick");
                                            kickProgress = result.Progress;
                                        }
                                        if (gesture.Name.Equals(this.hadukProgressGestureName)){
                                            //Console.WriteLine("_________________" + result.Progress);
                                            hadukProgress = result.Progress;
                                        }
                                    }
                                }
                            }
                            */

                            // if (gesture.Name.Equals(this.hadukenGestureName) && gesture.GestureType == GestureType.Discrete)
                            // {
                            //     DiscreteGestureResult result = null;
                            //     discreteResults.TryGetValue(gesture, out result);

                            //     if (result != null)
                            //     {
                            //         // update the GestureResultView object with new gesture result values
                            //         this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                            //         if (result.Confidence > 0.5)
                            //         {
                            //             Console.WriteLine("Haduken");
                            //             Console.WriteLine(result.Detected);
                            //             Console.WriteLine(result.Confidence);
                            //             SendKeys.SendWait("{Z}");
                            //         }
                            //     }
                            // }

                            // else if (gesture.Name.Equals(this.punchGestureName) && gesture.GestureType == GestureType.Discrete)
                            // {
                            //     DiscreteGestureResult result = null;
                            //     discreteResults.TryGetValue(gesture, out result);

                            //     if (result != null)
                            //     {
                            //         // update the GestureResultView object with new gesture result values
                            //         this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                            //         if (result.Confidence > 0.8)
                            //         {
                            //             Console.WriteLine("Punch");
                            //             Console.WriteLine(result.Detected);
                            //             Console.WriteLine(result.Confidence);
                            //             SendKeys.SendWait("{K}");
                            //         }
                            //     }
                            // }
                            
                            // if (gesture.Name.Equals(this.kickGestureName) && gesture.GestureType == GestureType.Discrete)
                            // {
                            //     DiscreteGestureResult result = null;
                            //     discreteResults.TryGetValue(gesture, out result);

                            //     if (result != null)
                            //     {
                            //         // update the GestureResultView object with new gesture result values
                            //         this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                            //         if (result.Confidence > 0.005)
                            //         {
                            //             Console.WriteLine("Kick");
                            //             Console.WriteLine(result.Detected);
                            //             Console.WriteLine(result.Confidence);
                            //             SendKeys.SendWait("{I}");
                            //         }
                            //     }
                            // }
                            
                            
                            
                        }

                        if(punchStarted){
                            punchStarted = true;
                        }
                        if(kickStarted){
                            kickStarted = true;
                        }
                        if(hadukStarted){
                            hadukStarted = true;
                        }
                        /*
                        if(punchProgress < 0){
                            punchProgress = 0;
                        }
                        else if (punchProgress > 1){
                            punchProgress= 1;
                        }

                        if(kickProgress < 0){
                            kickProgress = 0;
                        }
                        else if (kickProgress > 1){
                            kickProgress = 1;
                        }

                        if(hadukProgress < 0){
                            hadukProgress = 0;
                        }
                        else if (hadukProgress > 1){
                            hadukProgress = 1;
                        }
                        */

                        //Console.WriteLine("Punch Started: " + punchStarted+ " PunchProgress: " + punchProgress);
                        //Console.WriteLine("Kick Started: " + kickStarted + " KickProgress: " + kickProgress);
                        //Console.WriteLine("Haduk Started: " + hadukStarted + " hadukProgress: " + hadukProgress);
                        //Console.WriteLine("Punch Confidence: " + punchConfidence);
                        //Console.WriteLine("Haduken Confidence: " + hadukConfidence);

                        if (punchStarted && (punchConfidence >= hadukConfidence))
                        {
                            Console.WriteLine("Punch Thrown");
                            punchStarted= false;
                            punchConfidence = 0;
                            hadukConfidence= 0;
                            punchProgress = 0;
                            SendKeys.SendWait("{K}");
                        }
                        else if (kickStarted)
                        {
                            Console.WriteLine("Kick Thrown");
                            kickStarted= false;
                            kickProgress = 0;
                            SendKeys.SendWait("{I}");
                        }
                        else if (hadukStarted && (hadukConfidence <= hadukConfidence))
                        {
                            Console.WriteLine("HADUKEN!!");
                            hadukStarted= false;
                            hadukProgress = 0;
                            hadukConfidence= 0;
                            punchConfidence= 0;
                            SendKeys.SendWait("{Z}");
                        }
                        else if (jumpingJacksStarted)
                        {
                            Console.WriteLine("JUMP");
                            jumpingJacksStarted= false;
                            jumpingJacksConfidence= 0;
                            SendKeys.SendWait("{W}");
                        }
                        
                        this.GestureResultView.UpdateGestureResult(true, punchStarted, kickStarted, hadukStarted, punchProgress, kickProgress, hadukProgress);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            // update the GestureResultView object to show the 'Not Tracked' image in the UI
            this.GestureResultView.UpdateGestureResult(false, false, false, false, 0.0f, 0.0f, 0.0f);
        }
    }
}
