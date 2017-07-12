using System;
using System.Windows;

namespace Notes.Config
{
    [Serializable]
    public class WindowState
    {
        private double _left;
        private double _top;

        public WindowState()
        {
            Width = 300;
            Height = 540;
           _top = 0;
            _left = 0;
            AutoHideFactor = 10.0;
        }

        public double Width { get; set; }

        public double Height { get; set; }

        public double Left
        {
            get
            {
                if (_left < 0)
                {
                    _left = 0;
                }
                else if (_left > SystemParameters.VirtualScreenWidth - Width)
                {
                    _left = SystemParameters.VirtualScreenWidth - Width;
                }
                return _left;
            }
            set
            {
                if (value < 0)
                {
                    _left = 0;
                }
                else if (value > SystemParameters.VirtualScreenWidth - Width)
                {
                    _left = SystemParameters.VirtualScreenWidth - Width;
                }
                else
                {
                    _left = value;
                }

            }
        }

        public double Top
        {
            get
            {
                if (_top < 0)
                {
                    _top = 0;
                }
                else if (_top > SystemParameters.VirtualScreenHeight - Height)
                {
                    _top = SystemParameters.VirtualScreenHeight - Height;
                }
                return _top;
            }
            set
            {
                if (value < 0)
                {
                    _top = 0;
                }
                else if (value > SystemParameters.VirtualScreenHeight - Height)
                {
                    _top = SystemParameters.VirtualScreenHeight - Height;
                }
                else
                {
                    _top = value;
                }
            }
        }

        public double AutoHideFactor { get; set; }
    }
}