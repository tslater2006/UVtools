using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Timers;
using UVtools.Core.Operations;
using UVtools.UI.Controls.Tools;
using UVtools.UI.Extensions;
using UVtools.UI.Windows;

namespace UVtools.UI.Controls.Calibrators
{
    public partial class CalibrateBloomingEffectControl : ToolControl
    {
        public OperationCalibrateBloomingEffect Operation => (BaseOperation as OperationCalibrateBloomingEffect)!;

        private readonly Timer _timer = null!;

        private Bitmap? _previewImage;
        public Bitmap? PreviewImage
        {
            get => _previewImage;
            set => RaiseAndSetIfChanged(ref _previewImage, value);
        }

        public CalibrateBloomingEffectControl()
        {
            BaseOperation = new OperationCalibrateBloomingEffect(SlicerFile!);
            if (!ValidateSpawn()) return;

            InitializeComponent();


            _timer = new Timer(20)
            {
                AutoReset = false
            };
            _timer.Elapsed += (sender, e) => Dispatcher.UIThread.InvokeAsync(UpdatePreview);
        }

        public override void Callback(ToolWindow.Callbacks callback)
        {
            if (App.SlicerFile is null) return;
            switch (callback)
            {
                case ToolWindow.Callbacks.Init:
                case ToolWindow.Callbacks.AfterLoadProfile:
                    Operation.PropertyChanged += (sender, e) =>
                    {
                        _timer.Stop();
                        _timer.Start();
                    };
                    _timer.Stop();
                    _timer.Start();
                    break;
            }
        }

        public void UpdatePreview()
        {
            using var mat = Operation.GetLayerPreview();
            _previewImage?.Dispose();
            PreviewImage = mat.ToBitmapParallel();
        }
    }
}