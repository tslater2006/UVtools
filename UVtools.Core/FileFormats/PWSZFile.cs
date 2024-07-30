﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using UVtools.Core.Converters;
using UVtools.Core.Extensions;
using UVtools.Core.Layers;
using UVtools.Core.Operations;

namespace UVtools.Core.FileFormats;

public sealed class PWSZFile : FileFormat
{
    #region Sub classes

    public sealed class SettingsManifest
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "3";

        [JsonPropertyName("machine_type")]
        public SettingsMachineType MachineType { get; set; } = new ();

        [JsonPropertyName("machine_extern")]
        public SettingsMachineExtern MachineExtern { get; set; } = new();

        public override string ToString()
        {
            return
                $"{nameof(Version)}: {Version}, {nameof(MachineType)}: {MachineType}, {nameof(MachineExtern)}: {MachineExtern}";
        }
    }

    public sealed class SettingsChildScreen
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public uint Width { get; set; }

        [JsonPropertyName("height")]
        public uint Height { get; set; }

        public SettingsChildScreen()
        {
        }

        public SettingsChildScreen(int x, int y, uint width, uint height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public sealed class SettingsMachineType
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "3";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unknown";

        [JsonPropertyName("key_suffix")]
        public string KeySuffix { get; set; } = "pwsz";

        [JsonPropertyName("key_image_format")]
        public string KeyImageFormat { get; set; } = "pwszImg";

        [JsonPropertyName("res_x")]
        public uint ResolutionX { get; set; }

        [JsonPropertyName("res_y")]
        public uint ResolutionY { get; set; }

        [JsonPropertyName("xy_pixel")]
        public float PixelWidthMicrons { get; set; }

        [JsonPropertyName("xy_pixel_y")]
        public float PixelHeightMicrons { get; set; }

        [JsonPropertyName("max_samples")]
        public byte AntiAliasing { get; set; }

        [JsonPropertyName("property")]
        public ushort Properties { get; set; } = 119;

        [JsonPropertyName("print_xsize")]
        public float DisplayWidth { get; set; }

        [JsonPropertyName("print_ysize")]
        public float DisplayHeight { get; set; }

        [JsonPropertyName("print_zsize")]
        public float MachineZ { get; set; }

        [JsonPropertyName("max_file_version")]
        public uint MaxFileVersion { get; set; } = 518;

        [JsonPropertyName("prev_back_color")]  
        public float[] PreviewBackgroundColor { get; set; } = { 0.0f, 0.28f, 0.39f };

        [JsonPropertyName("prev_model_color")]
        public float[] ModelBackgroundColor { get; set; } = { 0.80f, 0.80f, 0.80f };

        [JsonPropertyName("prev_supports_color")]
        public float[] SupportsBackgroundColor { get; set; } = { 0.07f, 0.93f, 0.93f };

        [JsonPropertyName("prev_image_size")]
        public int[] PreviewImageSize { get; set; } = { 224, 168 };

        [JsonPropertyName("child_screen")]
        public SettingsChildScreen[] Screens { get; set; } = Array.Empty<SettingsChildScreen>();

        [JsonPropertyName("prev2_back_color")]
        public float[] Preview2BackgroundColor { get; set; } = { 0.08f, 0.11f, 0.16f };

        [JsonPropertyName("prev2_image_size")]
        public int[] Preview2ImageSize { get; set; } = { 336, 252 };

        [JsonPropertyName("raster_segments_capacity")]
        public uint RasterSegmentsCapacity { get; set; } = 100000;

        [JsonPropertyName("raster_antialiasing")]
        public byte RasterAntialiasing { get; set; } = 4;

        [JsonPropertyName("cloudprev_back_color")]
        public float[] CloudBackgroundColor { get; set; } = { 0.00f, 0.28f, 0.39f };

        [JsonPropertyName("cloudprev_imag_size")]
        public int[] CloudImageSize { get; set; } = { 800, 600 };

        public override string ToString()
        {
            return
                $"{nameof(Version)}: {Version}, {nameof(Name)}: {Name}, {nameof(KeySuffix)}: {KeySuffix}, {nameof(KeyImageFormat)}: {KeyImageFormat}, {nameof(ResolutionX)}: {ResolutionX}, {nameof(ResolutionY)}: {ResolutionY}, {nameof(PixelWidthMicrons)}: {PixelWidthMicrons}, {nameof(PixelHeightMicrons)}: {PixelHeightMicrons}, {nameof(AntiAliasing)}: {AntiAliasing}, {nameof(Properties)}: {Properties}, {nameof(DisplayWidth)}: {DisplayWidth}, {nameof(DisplayHeight)}: {DisplayHeight}, {nameof(MachineZ)}: {MachineZ}, {nameof(MaxFileVersion)}: {MaxFileVersion}, {nameof(PreviewBackgroundColor)}: {PreviewBackgroundColor}, {nameof(ModelBackgroundColor)}: {ModelBackgroundColor}, {nameof(SupportsBackgroundColor)}: {SupportsBackgroundColor}, {nameof(PreviewImageSize)}: {PreviewImageSize}, {nameof(Preview2BackgroundColor)}: {Preview2BackgroundColor}, {nameof(Preview2ImageSize)}: {Preview2ImageSize}, {nameof(RasterSegmentsCapacity)}: {RasterSegmentsCapacity}, {nameof(RasterAntialiasing)}: {RasterAntialiasing}, {nameof(CloudBackgroundColor)}: {CloudBackgroundColor}, {nameof(CloudImageSize)}: {CloudImageSize}";
        }
    }

    public sealed class SettingsMachineExtern
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "3";

        [JsonPropertyName("alias")]
        public string Alias { get; set; } = "Unknown";

        [JsonPropertyName("picture")]
        public string Picture { get; set; } = "Unknown.png";

        [JsonPropertyName("cloud_property")]
        public uint CloudProperty { get; set; }

        [JsonPropertyName("device_cn_code")]
        public string DeviceCnCode { get; set; } = string.Empty;

        [JsonPropertyName("factory_resins")]
        public object[] FactoryResins { get; set; } = Array.Empty<object>();

        [JsonPropertyName("user_resins")]
        public object[] UserResins { get; set; } = Array.Empty<object>();

        [JsonPropertyName("active_resins")]
        public string[] ActiveResins { get; set; } = { "default_resin" };

        [JsonPropertyName("firmware_calc_print_time")]
        public byte FirmwareCalcPrintTime { get; set; } = 1;

        [JsonPropertyName("firmware_calc_print_time_paras")]
        public object FirmwareCalcPrintParameters { get; set; } = new();

        [JsonPropertyName("firmware_calc_exp_time_paras")]
        public object FirmwareCalcExposureTimeParameters { get; set; } = new();

        public override string ToString()
        {
            return
                $"{nameof(Version)}: {Version}, {nameof(Alias)}: {Alias}, {nameof(Picture)}: {Picture}, {nameof(CloudProperty)}: {CloudProperty}, {nameof(DeviceCnCode)}: {DeviceCnCode}, {nameof(FactoryResins)}: {FactoryResins}, {nameof(UserResins)}: {UserResins}, {nameof(ActiveResins)}: {ActiveResins}, {nameof(FirmwareCalcPrintTime)}: {FirmwareCalcPrintTime}, {nameof(FirmwareCalcPrintParameters)}: {FirmwareCalcPrintParameters}, {nameof(FirmwareCalcExposureTimeParameters)}: {FirmwareCalcExposureTimeParameters}";
        }
    }

    public sealed class LayerManifest
    {
        [JsonPropertyName("exposure_time")]
        public float ExposureTime { get; set; }

        [JsonPropertyName("layer_index")]
        public uint LayerIndex { get; set; }

        [JsonPropertyName("layer_minheight")]
        public float LayerMinHeight { get; set; }

        [JsonPropertyName("layer_thickness")]
        public float LayerThickness { get; set; }

        [JsonPropertyName("zup_height")]
        public float LiftHeight { get; set; }

        [JsonPropertyName("zup_speed")]
        public float LiftSpeed { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(ExposureTime)}: {ExposureTime}, {nameof(LayerIndex)}: {LayerIndex}, {nameof(LayerMinHeight)}: {LayerMinHeight}, {nameof(LayerThickness)}: {LayerThickness}, {nameof(LiftHeight)}: {LiftHeight}, {nameof(LiftSpeed)}: {LiftSpeed}";
        }
    }

    public sealed class LayersControllerManifest
    {
        [JsonPropertyName("count")]
        public int Count => Layers.Length;

        [JsonPropertyName("paras")]
        public LayerManifest[] Layers { get; set; } = Array.Empty<LayerManifest>();
    }
    
    public sealed class PrintInfoManifest
    {
        [JsonPropertyName("cost")]
        public float Cost { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "€";

        [JsonPropertyName("print_time")]
        public float PrintTime { get; set; }

        [JsonPropertyName("volume")]
        public float Volume { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Cost)}: {Cost}, {nameof(Currency)}: {Currency}, {nameof(PrintTime)}: {PrintTime}, {nameof(Volume)}: {Volume}";
        }
    }


    public sealed class SoftwareInfoManifest
    {
        [JsonPropertyName("mark")]
        public string Mark { get; set; } = About.Software;

        [JsonPropertyName("opengl")]
        public string OpenGL { get; set; } = "3.3-CoreProfile";

        [JsonPropertyName("os")]
        public string OS { get; set; } = RuntimeInformation.RuntimeIdentifier;

        [JsonPropertyName("Version")]
        public string Version { get; set; } = About.VersionString;

        public override string ToString()
        {
            return
                $"{nameof(Mark)}: {Mark}, {nameof(OpenGL)}: {OpenGL}, {nameof(OS)}: {OS}, {nameof(Version)}: {Version}";
        }

        public void Update()
        {
            Mark = About.Software;
            Version = About.VersionString;
            OS = RuntimeInformation.RuntimeIdentifier;
        }
    }
    #endregion

    #region Constants
    private const string SettingsFileName = "anycubic_photon_resins.pwsp";
    private const string LayersFileName = "layers_controller.conf";
    private const string PrintInfoFileName = "print_info.json";
    private const string SoftwareInfoFileName = "software_info.conf";
    private const string SceneFileName = "scene.slice";
    #endregion

    #region Properties
    public SettingsManifest Settings { get; set; } = new ();
    public PrintInfoManifest PrintInfoSettings { get; set; } = new ();
    public LayersControllerManifest LayersSettings { get; set; } = new ();
    public SoftwareInfoManifest SoftwareInfoSettings { get; set; } = new ();

    public override FileFormatType FileType => FileFormatType.Archive;

    public override SpeedUnit FormatSpeedUnit => SpeedUnit.MillimetersPerSecond;

    public override PrintParameterModifier[] PrintParameterModifiers
    {
        get
        {
            return new[]
            {
                PrintParameterModifier.BottomLayerCount,
                PrintParameterModifier.TransitionLayerCount,

                PrintParameterModifier.BottomExposureTime,
                PrintParameterModifier.ExposureTime,

                PrintParameterModifier.BottomLiftHeight,
                PrintParameterModifier.BottomLiftSpeed,
                PrintParameterModifier.LiftHeight,
                PrintParameterModifier.LiftSpeed,
            };
        }
    }

    public override PrintParameterModifier[] PrintParameterPerLayerModifiers { get; } = {
        PrintParameterModifier.PositionZ,
        PrintParameterModifier.ExposureTime,
        PrintParameterModifier.LiftHeight,
        PrintParameterModifier.LiftSpeed,
    };

    public override string ConvertMenuGroup => "Anycubic Photon Workshop";

    public override FileExtension[] FileExtensions { get; } = {
        new(typeof(PWSZFile), "pm7", "Photon Mono M7 (PM7)"),
        new(typeof(PWSZFile), "pwsz", "Photon Mono M7 Pro (PWSZ)")
    };

    public override uint ResolutionX
    {
        get => Settings.MachineType.ResolutionX;
        set => base.ResolutionX = Settings.MachineType.ResolutionX = (ushort) value;
    }

    public override uint ResolutionY
    {
        get => Settings.MachineType.ResolutionY;
        set => base.ResolutionY = Settings.MachineType.ResolutionY = (ushort)value;
    }

    public override float DisplayWidth
    {
        get => Settings.MachineType.DisplayWidth;
        set => base.DisplayWidth = Settings.MachineType.DisplayWidth = RoundDisplaySize(value);
    }

    public override float DisplayHeight
    {
        get => Settings.MachineType.DisplayHeight;
        set => base.DisplayHeight = Settings.MachineType.DisplayHeight = RoundDisplaySize(value);
    }

    public override float MachineZ
    {
        get => Settings.MachineType.MachineZ;
        set => base.MachineZ = Settings.MachineType.MachineZ = value;
    }

    public override string MachineName
    {
        get => Settings.MachineType.Name;
        set
        {
            Settings.MachineExtern.Picture = $"{value.Replace(" ", string.Empty)}.png";
            base.MachineName = Settings.MachineExtern.Alias = Settings.MachineType.Name = value;
        }
    }

    public override Size[] ThumbnailsOriginalSize { get; } =
    {
        new(224, 168),
        new(336, 252)
    };


    public override object[] Configs => new object[] { 
        Settings, PrintInfoSettings, SoftwareInfoSettings
    };

    #endregion

    #region Constructor
    public PWSZFile()
    { }
    #endregion

    #region Methods

    protected override void OnBeforeEncode(bool isPartialEncode)
    {
        Settings.MachineType.PixelWidthMicrons = PixelWidthMicrons;
        Settings.MachineType.PixelHeightMicrons = PixelHeightMicrons;
        Settings.MachineType.PreviewImageSize = new []{ ThumbnailsOriginalSize[0].Width, ThumbnailsOriginalSize[0].Height };
        Settings.MachineType.Preview2ImageSize = new []{ ThumbnailsOriginalSize[1].Width, ThumbnailsOriginalSize[1].Height };

        if (Settings.MachineType.Screens.Length > 0)
        {
            Settings.MachineType.Screens[0].X = 0;
            Settings.MachineType.Screens[0].Y = 0;
            Settings.MachineType.Screens[0].Width = ResolutionX;
            Settings.MachineType.Screens[0].Height = ResolutionY;
        }
        else
        {
            Settings.MachineType.Screens = new[]
            {
                new SettingsChildScreen(0, 0, ResolutionX, ResolutionY)
            };
        }
        
        PrintInfoSettings.Cost = MaterialCost;
        PrintInfoSettings.PrintTime = PrintTime;
        PrintInfoSettings.Volume = Volume;
        SoftwareInfoSettings.Update();

        LayersSettings.Layers = new LayerManifest[LayerCount];
        float minHeight = 0;
        for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
        {
            var layer = this[layerIndex];
            var relativeZ = layer.RelativePositionZ;
            LayersSettings.Layers[layerIndex] = new LayerManifest
            {
                ExposureTime = layer.ExposureTime,
                LayerIndex = layer.Index,
                LayerMinHeight = Layer.RoundHeight(minHeight),
                LayerThickness = relativeZ,
                LiftHeight = layer.LiftHeight,
                LiftSpeed = SpeedConverter.Convert(layer.LiftSpeed, CoreSpeedUnit, FormatSpeedUnit)
            };

            minHeight += relativeZ;
        }
    }

    protected override void EncodeInternally(OperationProgress progress)
    {
        using var outputFile = ZipFile.Open(TemporaryOutputFileFullPath, ZipArchiveMode.Create);

        EncodeAllThumbnailsInZip(outputFile, "preview_images/preview_{0}.png", progress);
        //EncodeLayersInZip(outputFile, IndexStartNumber.One, progress);

        outputFile.CreateEntryFromSerializeJson(SettingsFileName, Settings, ZipArchiveMode.Create, JsonExtensions.SettingsIndent);
        outputFile.CreateEntryFromSerializeJson(PrintInfoFileName, PrintInfoSettings, ZipArchiveMode.Create, JsonExtensions.SettingsIndent);
        outputFile.CreateEntryFromSerializeJson(LayersFileName, LayersSettings, ZipArchiveMode.Create, JsonExtensions.SettingsIndent);
        outputFile.CreateEntryFromSerializeJson(SoftwareInfoFileName, SoftwareInfoSettings, ZipArchiveMode.Create, JsonExtensions.SettingsIndent);
    }

    protected override void DecodeInternally(OperationProgress progress)
    {
        using var inputFile = ZipFile.Open(FileFullPath!, ZipArchiveMode.Read);
        var entry = inputFile.GetEntry(SettingsFileName);
        if (entry is null)
        {
            Clear();
            throw new FileLoadException($"Unable to find {SettingsFileName} file", FileFullPath);
        }
        try
        {
            using var stream = entry.Open();
            Settings = JsonSerializer.Deserialize<SettingsManifest>(stream)!;
        }
        catch (Exception e)
        {
            Clear();
            throw new FileLoadException($"Unable to deserialize '{entry.Name}'\n{e}", FileFullPath);
        }

        entry = inputFile.GetEntry(LayersFileName);
        if (entry is null)
        {
            Clear();
            throw new FileLoadException($"Unable to find {LayersFileName} file", FileFullPath);
        }
        try
        {
            using var stream = entry.Open();
            LayersSettings = JsonSerializer.Deserialize<LayersControllerManifest>(stream)!;
        }
        catch (Exception e)
        {
            Clear();
            throw new FileLoadException($"Unable to deserialize '{entry.Name}'\n{e}", FileFullPath);
        }

        if (LayersSettings.Count == 0)
        {
            Clear();
            throw new FileLoadException("Unable to detect layer images in the file", FileFullPath);
        }

        entry = inputFile.GetEntry(PrintInfoFileName);
        if (entry is not null)
        {
            try
            {
                using var stream = entry.Open();
                PrintInfoSettings = JsonSerializer.Deserialize<PrintInfoManifest>(stream)!;
            }
            catch (Exception e)
            {
                Clear();
                throw new FileLoadException($"Unable to deserialize '{entry.Name}'\n{e}", FileFullPath);
            }
        }

        entry = inputFile.GetEntry(SoftwareInfoFileName);
        if (entry is not null)
        {
            try
            {
                using var stream = entry.Open();
                SoftwareInfoSettings = JsonSerializer.Deserialize<SoftwareInfoManifest>(stream)!;
            }
            catch (Exception e)
            {
                Clear();
                throw new FileLoadException($"Unable to deserialize '{entry.Name}'\n{e}", FileFullPath);
            }
        }

        DecodeAllThumbnailsFromZip(inputFile, progress, "preview_");

        Init((uint)LayersSettings.Count);

        float positionZ = 0;
        for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
        {
            positionZ += LayersSettings.Layers[layerIndex].LayerThickness;
            this[layerIndex] = new Layer(layerIndex, this)
            {
                Index = layerIndex,
                PositionZ = Layer.RoundHeight(positionZ),
                ExposureTime = (float)Math.Round(LayersSettings.Layers[layerIndex].ExposureTime, 2, MidpointRounding.AwayFromZero),
                LiftHeight = (float)Math.Round(LayersSettings.Layers[layerIndex].LiftHeight, 2, MidpointRounding.AwayFromZero),
                LiftSpeed = SpeedConverter.Convert(LayersSettings.Layers[layerIndex].LiftSpeed, FormatSpeedUnit, CoreSpeedUnit)
            };
        }
        //DecodeLayersFromZip(inputFile, IndexStartNumber.One, progress);

        UpdateGlobalPropertiesFromLayers();
    }

    protected override void PartialSaveInternally(OperationProgress progress)
    {
        using var outputFile = ZipFile.Open(TemporaryOutputFileFullPath, ZipArchiveMode.Update);

        outputFile.CreateEntryFromSerializeJson(SettingsFileName, Settings, ZipArchiveMode.Update, JsonExtensions.SettingsIndent);
        outputFile.CreateEntryFromSerializeJson(PrintInfoFileName, PrintInfoSettings, ZipArchiveMode.Update, JsonExtensions.SettingsIndent);
        outputFile.CreateEntryFromSerializeJson(LayersFileName, LayersSettings, ZipArchiveMode.Update, JsonExtensions.SettingsIndent);
        outputFile.CreateEntryFromSerializeJson(SoftwareInfoFileName, SoftwareInfoSettings, ZipArchiveMode.Update, JsonExtensions.SettingsIndent);
    }
    #endregion
}