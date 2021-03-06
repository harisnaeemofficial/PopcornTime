﻿#region License

// Copyright (c) 2015 Harold Martinez-Molina <hanthonym@outlook.com>
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace PopcornTime.Common
{
    public class UiBlocker : IDisposable
    {
        private static UiBlocker _current;
        private Popup _popup;
        private bool _shown;
        private TextBlock _subMsg;

        public UiBlocker()
        {
            CreatePopup();
        }

        public void Dispose()
        {
            if (_shown)
                HideAnimation();
            else
                Dismiss();
        }

        public void Dismiss()
        {
            try
            {
                if (_popup != null)
                    _popup.IsOpen = false;
                _popup = null;
                _current = null;
            }
            catch
            {
                // ignored
            }
        }

        public static UiBlocker Show(string msg)
        {
            var blocker = InternalCreate(msg);
            blocker.ShowPopup();
            return blocker;
        }

        private static UiBlocker InternalCreate(string msg)
        {
            _current?.Dismiss();

            var curtain = new UiBlocker();
            curtain.UpdateProgress(msg);

            _current = curtain;
            return curtain;
        }

        private void CreatePopup()
        {
            _popup = new Popup
            {
                RenderTransform = new TranslateTransform(),
                Opacity = 0
            };

            var grid = new Grid
            {
                Background = new SolidColorBrush(Colors.Gray),
                Width = Window.Current.Bounds.Width,
                Height = Window.Current.Bounds.Height
            };

            var panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            #region Message Text Block

            _subMsg = new TextBlock
            {
                Text = "Loading...",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                TextWrapping = TextWrapping.Wrap
            };

            #endregion

            #region Progress bar

            var progress = new ProgressBar {IsIndeterminate = true};

            #endregion

            panel.Children.Add(_subMsg);
            panel.Children.Add(progress);
            grid.Children.Add(panel);

            _popup.Child = grid;
            _popup.IsOpen = true;

            //Make the framework (re)calculate the size of the element
            grid.Measure(new Size(double.MaxValue, double.MaxValue));
        }

        private void ShowPopup()
        {
            _shown = true;
            //Animate transition
            var slideDownAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new SineEase()
            };

            var sb = new Storyboard();
            sb.Children.Add(slideDownAnimation);
            Storyboard.SetTarget(slideDownAnimation, _popup);
            Storyboard.SetTargetProperty(slideDownAnimation, "(UIElement.Opacity)");

            sb.Begin();
        }

        private void HideAnimation()
        {
            if (_popup == null) return;
            //Animate transition
            var slideAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                EasingFunction = new SineEase()
            };

            var sbHide = new Storyboard();
            sbHide.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, _popup);
            Storyboard.SetTargetProperty(slideAnimation, "(UIElement.Opacity)");
            sbHide.Completed += (s, h) =>
            {
                try
                {
                    Dismiss();
                }
                catch
                {
                    // ignored
                }
            };
            sbHide.Begin();
        }

        public void UpdateProgress(string message)
        {
            _subMsg.Text = message;

            if (!_shown)
                ShowPopup();
        }

        public void UpdateProgress(string message, int progress, int progressMax = 1)
        {
            UpdateProgress(message);
        }
    }
}