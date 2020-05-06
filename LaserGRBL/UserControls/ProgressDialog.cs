using System;
using System.Threading;
using System.Windows.Forms;

namespace LaserGRBL.UserControls {

    public static class ProgressDialog {

        private static int mProgressValue = 0;
        private static int mProgressMaximum = 0;
        private static string mFormTitle = "Please Wait";
        private static bool mVisible = false;

        private static readonly object mutex = new object();

        public static string Text {
            get {

                lock (mutex) {
                    return mFormTitle;
                }
            }
            set {
                lock (mutex) {

                    mFormTitle = value;
                }
            }
        }

        public static int Maximum {
            get {

                lock (mutex) {
                    return mProgressMaximum;
                }
            }
            set {
                lock (mutex) {

                    mProgressValue = 0;
                    mProgressMaximum = Math.Max(0, value);
                }
            }
        }

        public static int Progress {
            get {

                lock (mutex) {
                    return mProgressValue;
                }
            }
            set {
                lock (mutex) {

                    mProgressValue = Math.Max(0, Math.Min(mProgressMaximum, value));
                }
            }
        }

        public static bool Visible {
            get {

                lock (mutex) {
                    return mVisible;
                }
            }
            set {

                lock (mutex) {

                    if (value) {
                                           
                        Show();
                    } else {
                        mVisible = false;
                    }        
                }                
            }
        }
         
        private static void Show() {

            lock (mutex) {

                // Already Visible?
                if (mVisible) {

                    return;
                }
                mVisible = true;

                // Create From
                var form = new Form() {
                    Name = "UserControlsProgressDialogForm",
                    Text = mFormTitle,
                    ControlBox = false,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 240,
                    Height = 5,
                    Enabled = true
                };
 
                // Create Progress Bar
                var progressBar = new ProgressBar() {

                    Style = ProgressBarStyle.Marquee,
                    Parent = form,
                    Dock = DockStyle.Fill,
                    Enabled = true,
                    Maximum = mProgressMaximum,
                    Value = mProgressValue,
                };

                // Form Invalidate Handler
                form.Invalidated += new InvalidateEventHandler((Sender, e) =>
                {
                    lock (mutex) {

                        form.Text = mFormTitle;
                    }
                });

                // Progress Bar Invalidate Handler
                progressBar.Invalidated += new InvalidateEventHandler((Sender, e) =>
                {

                    lock (mutex) {
                         
                        progressBar.Style = mProgressMaximum > 0 ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;    
                        if (mProgressMaximum == 0) {

                            progressBar.Style = ProgressBarStyle.Marquee;
                            progressBar.Value = 0;
                            progressBar.Maximum = 100;
                        } else {

                            progressBar.Maximum = mProgressMaximum;
                            progressBar.Value = mProgressValue;
                        } 
                    }
                });

                // Main Loop
                form.Shown += new EventHandler((Sender, e) =>
                {

                    var visible = true;
                    while (visible) {

                        Thread.Sleep(100);
                        lock (mutex) {

                            visible = mVisible;
                        }

                        //Update Form
                        progressBar.Invalidate();
                        progressBar.Update();
                        form.Invalidate();
                        form.Update();                        
                    }

                    //Close
                    form.Close();

                    //Cleanup
                    lock (mutex) {

                        mVisible = false;
                        form = null;
                        progressBar = null;
                    }
                });

                // Start Modal Dialog Thread
                (new Thread(() => { form.ShowDialog(); }) {

                    Priority = ThreadPriority.Normal,
                }).Start();

            }
        }
   
    }
}