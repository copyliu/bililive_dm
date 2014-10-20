using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Bililive_dm.Annotations;

namespace Bililive_dm
{
    [Serializable]
    public class StoreModel : INotifyPropertyChanged
    {
        public double MainOverlayXoffset
        {
            get { return _mainOverlayXoffset; }
            set
            {
                if (value.Equals(_mainOverlayXoffset)) return;
                _mainOverlayXoffset = Store.MainOverlayXoffset = value;

                ((MainWindow) (Application.Current.MainWindow)).overlay.Top = SystemParameters.WorkArea.Top + value;

                OnPropertyChanged();
            }
        }

        public double MainOverlayYoffset
        {
            get { return _mainOverlayYoffset; }
            set
            {
                if (value.Equals(_mainOverlayYoffset)) return;
                _mainOverlayYoffset = Store.MainOverlayYoffset = value;
                ((MainWindow) (Application.Current.MainWindow)).overlay.Left = SystemParameters.WorkArea.Right -
                                                                               Store.MainOverlayWidth + value;
                OnPropertyChanged();
            }
        }

        public double MainOverlayWidth
        {
            get { return _mainOverlayWidth; }
            set
            {
                if (value.Equals(_mainOverlayWidth)) return;
                _mainOverlayWidth = Store.MainOverlayWidth = value;
                ((MainWindow) (Application.Current.MainWindow)).overlay.Width = value;
                ((MainWindow) (Application.Current.MainWindow)).overlay.Left = SystemParameters.WorkArea.Right -
                                                                               value + Store.MainOverlayYoffset;
                OnPropertyChanged();
            }
        }

        public double MainOverlayEffect1
        {
            get { return _mainOverlayEffect1; }
            set
            {
                if (value.Equals(_mainOverlayEffect1)) return;
                _mainOverlayEffect1 = Store.MainOverlayEffect1 = value;
                OnPropertyChanged();
            }
        }

        public double MainOverlayEffect2
        {
            get { return _mainOverlayEffect2; }
            set
            {
                if (value.Equals(_mainOverlayEffect2)) return;
                _mainOverlayEffect2 = Store.MainOverlayEffect2 = value;
                OnPropertyChanged();
            }
        }

        public double MainOverlayEffect3
        {
            get { return _mainOverlayEffect3; }
            set
            {
                if (value.Equals(_mainOverlayEffect3)) return;
                _mainOverlayEffect3 = Store.MainOverlayEffect3 = value;
                OnPropertyChanged();
            }
        }

        public double MainOverlayEffect4
        {
            get { return _mainOverlayEffect4; }
            set
            {
                if (value.Equals(_mainOverlayEffect4)) return;
                _mainOverlayEffect4 = Store.MainOverlayEffect4 = value;
                OnPropertyChanged();
            }
        }

        public double MainOverlayFontsize
        {
            get { return _mainOverlayFontsize; }
            set
            {
                if (value.Equals(_mainOverlayFontsize)) return;
                _mainOverlayFontsize = Store.MainOverlayFontsize = value;
                OnPropertyChanged();
            }
        }

        public double FullOverlayEffect1
        {
            get { return _fullOverlayEffect1; }
            set
            {
                if (value.Equals(_fullOverlayEffect1)) return;
                Store.FullOverlayEffect1 = _fullOverlayEffect1 = value;
                OnPropertyChanged();
            }
        }

        public double FullOverlayFontsize
        {
            get { return _fullOverlayFontsize; }
            set
            {
                if (value.Equals(_fullOverlayFontsize)) return;
                Store.FullOverlayFontsize = _fullOverlayFontsize = value;
                OnPropertyChanged();
            }
        }

        private double _mainOverlayYoffset;
        private double _mainOverlayWidth;
        private double _mainOverlayEffect1; //拉伸
        private double _mainOverlayEffect2; //文字出現
        private double _mainOverlayEffect3; //文字停留
        private double _mainOverlayEffect4; //窗口消失
        private double _mainOverlayFontsize;


        private double _fullOverlayEffect1; //文字速度
        private double _fullOverlayFontsize;
        private double _mainOverlayXoffset;

        public StoreModel()
        {
            _fullOverlayFontsize = Store.FullOverlayFontsize;
            _fullOverlayEffect1 = Store.FullOverlayEffect1;
            _mainOverlayFontsize = Store.MainOverlayFontsize;
            _mainOverlayEffect4 = Store.MainOverlayEffect4;
            _mainOverlayEffect3 = Store.MainOverlayEffect3;
            _mainOverlayEffect2 = Store.MainOverlayEffect2;
            _mainOverlayEffect1 = Store.MainOverlayEffect1;
            _mainOverlayWidth = Store.MainOverlayWidth;
            _mainOverlayXoffset = Store.MainOverlayXoffset;
            _mainOverlayYoffset = Store.MainOverlayYoffset;
        }

        public void toStatic()
        {
            Store.FullOverlayFontsize = FullOverlayFontsize;
            Store.FullOverlayEffect1 = FullOverlayEffect1;
            Store.MainOverlayFontsize = MainOverlayFontsize;
            Store.MainOverlayEffect4 = MainOverlayEffect4;
            Store.MainOverlayEffect3 = MainOverlayEffect3;
            Store.MainOverlayEffect2 = MainOverlayEffect2;
            Store.MainOverlayEffect1 = MainOverlayEffect1;
            Store.MainOverlayWidth = MainOverlayWidth;
            Store.MainOverlayXoffset = MainOverlayXoffset;
            Store.MainOverlayYoffset = MainOverlayYoffset;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}