using System.ComponentModel;

namespace VGC50x.Utils
{
    public class ViewData : INotifyPropertyChanged
    {
        private bool m_ctrl_btn_enabled = false;

        public bool CtrlBtnEnabled
        {
            get { return m_ctrl_btn_enabled; }

            set
            {
                if (value != m_ctrl_btn_enabled)
                {
                    m_ctrl_btn_enabled = value;
                    Notify(nameof(m_ctrl_btn_enabled));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}