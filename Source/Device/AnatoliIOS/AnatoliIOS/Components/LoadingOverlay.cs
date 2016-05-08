using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace AnatoliIOS.Components
{
    public class LoadingOverlay : UIView
    {
        // control declarations
        UIActivityIndicatorView activitySpinner;
        UILabel loadingLabel;

        public LoadingOverlay(CGRect frame, bool cancelable = false)
            : base(frame)
        {
            // configurable bits
            BackgroundColor = UIColor.Black;
            Alpha = 0.75f;
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat labelHeight = 22;
            nfloat labelWidth = Frame.Width - 20;

            // derive the center x and y
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // create the activity spinner, center it horizontall and put it 5 points above center x
            activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new CGRect(
                centerX - (activitySpinner.Frame.Width / 2),
                centerY - activitySpinner.Frame.Height - 20,
                activitySpinner.Frame.Width,
                activitySpinner.Frame.Height);
            activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(activitySpinner);
            activitySpinner.StartAnimating();

            // create and configure the "Loading Data" label
            loadingLabel = new UILabel(new CGRect(
                centerX - (labelWidth / 2),
                centerY + 20,
                labelWidth,
                labelHeight
                ));
            loadingLabel.BackgroundColor = UIColor.Clear;
            loadingLabel.TextColor = UIColor.White;
            loadingLabel.Text = "لطفا صبور باشید";
            loadingLabel.Font = UIFont.FromName("IRAN", 13);
            loadingLabel.TextAlignment = UITextAlignment.Center;
            loadingLabel.AutoresizingMask = UIViewAutoresizing.All;
            var cancelButton = new UIButton(new CGRect(
                centerX - (labelWidth / 2),
                centerY + 50,
                labelWidth,
                labelHeight
                ));

            cancelButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            cancelButton.SetTitle("بی خیال", UIControlState.Normal);
            cancelButton.Font = UIFont.FromName("IRAN", 13);
            cancelButton.AutoresizingMask = UIViewAutoresizing.All;
            cancelButton.TouchUpInside += delegate
            {
                if (Canceled != null)
                {
                    Canceled.Invoke(this, new EventArgs());
                }
            };
            AddSubview(loadingLabel);
            if (cancelable)
            {
                AddSubview(cancelButton);
            }
        }

        public EventHandler Canceled;

        /// <summary>
        /// Fades out the control and then removes it from the super view
        /// </summary>
        public void Hide()
        {
            UIView.Animate(
                0.5, // duration
                () => { Alpha = 0; },
                () => { RemoveFromSuperview(); }
            );
        }
    }
}
