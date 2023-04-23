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
    using System.Windows.Media.Media3D;
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

        // Create a list to store the previously tracked body IDs
        // This list will be used to track multiple in-game players
        List<ulong> trackedBodyIds = new List<ulong>();

        bool punchStarted = false;
        bool kickStarted = false;
        bool hadukStarted = false;
        bool jumpingJacksStarted = false;
        float punchProgress = 0.0f;
        float kickProgress = 0.0f;
        float hadukProgress = 0.0f;
        float punchConfidence= 0.0f;
        float kickConfidence = 0.0f;
        float hadukConfidence = 0.0f;
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
                            // Check if this body is already being tracked
                            if (!trackedBodyIds.Contains(body.TrackingId))
                            {
                                // Add the new body to the list
                                trackedBodyIds.Add(body.TrackingId);
                            }
                            int index = trackedBodyIds.IndexOf(body.TrackingId);

                            //Console.WriteLine("Body Index: " + (index % 2));
                            // Calculate the lean angle of the upper body relative to the lower body
                            CameraSpacePoint leftHip = body.Joints[JointType.HipLeft].Position;
                            CameraSpacePoint rightHip = body.Joints[JointType.HipRight].Position;
                            CameraSpacePoint leftShoulder = body.Joints[JointType.ShoulderLeft].Position;
                            CameraSpacePoint rightShoulder = body.Joints[JointType.ShoulderRight].Position;

                            Vector3D hipVector = (rightHip.ToVector3D() - leftHip.ToVector3D()).Normalized();
                            Vector3D shoulderVector = (rightShoulder.ToVector3D() - leftShoulder.ToVector3D()).Normalized();

                            // Calculate the cross product of the hip and shoulder vectors
                            Vector3D crossProduct = Vector3D.CrossProduct(hipVector, shoulderVector);
                            double leanAngle = 0.0;
                            // Calculate the signed lean angle
                            try
                            {
                                leanAngle = Vector3D.AngleBetween(hipVector, shoulderVector) * Math.Sign(crossProduct.Y);
                            }
                            catch (ArithmeticException ex)
                            {
                                return;
                            }

                            //Console.WriteLine("LEAN_ANGLE: " + leanAngle);
                            // Player 1
                            if ((index % 2) == 0) {
                                if (leanAngle >= 10.0f)
                                {
                                    Console.WriteLine("<<<<<<<<<<=================");
                                    SendKeys.SendWait("{A}");
                                }
                                else if (leanAngle <= -10.0f)
                                {
                                    Console.WriteLine("===============>>>>>>>>>>>>");
                                    SendKeys.SendWait("{D 10}");
                                }
                            }
                            // Player 2
                            else
                            {
                                if (leanAngle >= 10.0f)
                                {
                                    Console.WriteLine("<<<<<<<<<<=================");
                                    SendKeys.SendWait("{LEFT 10}");
                                }
                                else if (leanAngle <= -10.0f)
                                {
                                    Console.WriteLine("===============>>>>>>>>>>>>");
                                    SendKeys.SendWait("{RIGHT}");
                                }
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

                    // Check if this body is already being tracked
                    if (!trackedBodyIds.Contains(frame.TrackingId))
                    {
                        // Add the new body to the list
                        trackedBodyIds.Add(frame.TrackingId);
                    }
                    int index = trackedBodyIds.IndexOf(frame.TrackingId);

                    if (discreteResults != null)
                    {

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

                        // Player One
                        if (index % 2 == 0)
                        {
                            if (punchStarted && (punchConfidence >= hadukConfidence))
                            {
                                Console.WriteLine("Punch Thrown");
                                punchStarted = false;
                                punchConfidence = 0;
                                hadukConfidence = 0;
                                punchProgress = 0;
                                SendKeys.SendWait("{K}");
                            }
                            else if (kickStarted)
                            {
                                Console.WriteLine("Kick Thrown");
                                kickStarted = false;
                                kickProgress = 0;
                                SendKeys.SendWait("{I}");
                            }
                            else if (hadukStarted && (hadukConfidence <= hadukConfidence))
                            {
                                Console.WriteLine("HADUKEN!!");
                                hadukStarted = false;
                                hadukProgress = 0;
                                hadukConfidence = 0;
                                punchConfidence = 0;
                                SendKeys.SendWait("{Z}");
                            }
                            else if (jumpingJacksStarted)
                            {
                                Console.WriteLine("JUMP");
                                jumpingJacksStarted = false;
                                jumpingJacksConfidence = 0;
                                SendKeys.SendWait("{W}");
                            }
                        }
                        // Player Two
                        else
                        {
                            if (punchStarted && (punchConfidence >= hadukConfidence))
                            {
                                Console.WriteLine("             Punch Thrown");
                                punchStarted = false;
                                punchConfidence = 0;
                                hadukConfidence = 0;
                                punchProgress = 0;
                            }
                            else if (kickStarted)
                            {
                                Console.WriteLine("             Kick Thrown");
                                kickStarted = false;
                                kickProgress = 0;
                            }
                            else if (hadukStarted && (hadukConfidence <= hadukConfidence))
                            {
                                Console.WriteLine("             HADUKEN!!");
                                hadukStarted = false;
                                hadukProgress = 0;
                                hadukConfidence = 0;
                                punchConfidence = 0;
                            }
                            else if (jumpingJacksStarted)
                            {
                                Console.WriteLine("             JUMP");
                                jumpingJacksStarted = false;
                                jumpingJacksConfidence = 0;
                                SendKeys.SendWait("{UP}");
                            }
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
