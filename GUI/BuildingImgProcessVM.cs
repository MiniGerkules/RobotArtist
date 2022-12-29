using GUI.PLT;
using System.ComponentModel;

namespace GUI {
    internal class BuildingImgProcessVM : NotifierOfPropertyChange {
        private readonly PLTDecoder decoder;
        private readonly PLTImgBuilder builder;
        private string status;

        public string Status => status;
        public int MaxVal => PLTDecoder.MaxPercent + PLTImgBuilder.MaxPercent;
        public int CurVal => decoder.CurPercent + builder.CurPercent;

        public BuildingImgProcessVM(PLTDecoder decoder, PLTImgBuilder builder) {
            this.decoder = decoder;
            this.builder = builder;

            this.decoder.PropertyChanged += Handler;
            this.builder.PropertyChanged += Handler;
        }

        private void Handler(object sender, PropertyChangedEventArgs e) {
            if (sender is PLTDecoder) {
                status = "Process PLT file";
            } else if (sender is PLTImgBuilder recived) {
                if (recived.CurPercent != PLTImgBuilder.MaxPercent) {
                    status = "Render image from PLT file";
                } else {
                    status = "";
                    decoder.ResetCurPercent();
                    builder.ResetCurPercent();
                }
            }

            NotifyPropertyChanged(nameof(Status));
            NotifyPropertyChanged(nameof(CurVal));
        }
    }
}
