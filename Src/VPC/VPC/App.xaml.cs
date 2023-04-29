namespace VPC;

public partial class App : Application
{
  //SpeechSynthesizer _ss; public SpeechSynthesizer Synth { get { if (_ss == null) { _ss = new SpeechSynthesizer(); _ss.SpeakAsyncCancelAll(); _ss.Rate = 6; _ss.Volume = 75; _ss.SelectVoiceByHints(gender: VoiceGender.Female); } return _ss; } }

  VPViewModel? _viewModel;
  MainPlayerWindow? _view;
  readonly AppSettings _settings = AppSettings.Instance;
  readonly DateTime _daySsnStarted = DateTime.Today;

  protected override async void OnStartup(StartupEventArgs e)
  {
    Application.Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox)?.SelectAll(); })); //tu: TextBox
    //2020Feb SetupTracingOptions("AAV.VPC", new TraceSwitch("OnlyUsedWhenInConfig", "This is the trace for all               messages... but who cares?   See ScrSvr for a model.") { Level = TraceLevel.Verbose });
    //DevOp.TestIsoAccessibilty("VPC");

    base.OnStartup(e);

    //var version = //System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();			////System.Deployment.			//Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;			//dbIni();

    init0(e, _view = new MainPlayerWindow { DataContext = new VPViewModel() }); // OnStartup_Simple(e);

    saveSettings(_view, _viewModel.PlayerMargin);

    await Task.Delay(222);
    Current.Shutdown();
  }
  protected override void OnExit(ExitEventArgs e)
  {
    Debug.Write($"{e}");
    if (_viewModel.VPModel.CrntMU != null) _settings.LastVideo = _viewModel.VPModel.CrntMU.PathFileCur;
    //Settings.Default.Save();

    _viewModel.LogSessionViewTime(_daySsnStarted);
  }

  //void OnStartup_Unity(StartupEventArgs e)
  //{
  //  IUnityContainer container = new UnityContainer();
  //  container.RegisterType<IVPViewModel, VPViewModel>();      //container.RegisterType<IPerson, Person>();			//container.RegisterType<ICustomer, Customer>();

  //  init0(e, container.Resolve<MainPlayerWindow>());
  //}

  void init0(StartupEventArgs args, MainPlayerWindow vw)
  {
    _viewModel = vw.DataContext as VPViewModel;
    _viewModel.Player = vw.wmp;
    _viewModel.Window = vw;
    _viewModel.LayoutRoot = vw.LayoutRoot;
    _viewModel.Fvc = vw.fuc;
    //closeEvent(window, _viewModel);

    //if (!string.IsNullOrEmpty(Settings.Default.AppSetting))
    //{
    //  _settings = Serializer.LoadFromString<AppSettings>(Settings.Default.AppSetting) as AppSettings;
    //  //vw.Left = _settings.windowLeft;
    //  //vw.Top = _settings.windowTop;
    //  //vw.Width = _settings.windowWidth;
    //  //vw.Height = _settings.windowHeight;
    //  _viewModel.PlayerMargin = _settings.PlayerMargin;
    //}

    autoPlay(args, _viewModel);

    _ = BindableBaseViewModel.ShowModalMvvm(_viewModel, vw);  // window.ShowDialog();
  }

  static void saveSettings(Window window, int playerMargin)
  {
    var stgs = new AppSettings
    {
      PlayerMargin = playerMargin
    };
    if (window.WindowState == WindowState.Normal)
    {
      stgs.windowTop = window.Top;
      stgs.windowLeft = window.Left;
      stgs.windowWidth = window.Width;
      stgs.windowHeight = window.Height;
    }
    //Settings.Default.AppSetting = Serializer.SaveToString(stgs);
    //Settings.Default.Save();
  }
  static void autoPlay(StartupEventArgs e, IVPViewModel viewModel)
  {
    string? mediaFileOrFolder;
    if (e.Args.Length > 0 && !string.IsNullOrEmpty(e.Args[0]))
      mediaFileOrFolder = e.Args[0];      //else if (!string.IsNullOrWhiteSpace(Settings.Default.LastVideo) && File.Exists(Settings.Default.LastVideo))				mediaFile = Settings.Default.LastVideo;
    else
    {
      mediaFileOrFolder = firstExistingFileFromSkyCache();
      if (mediaFileOrFolder == null) return;
      //else mediaFileOrFolder = Path.GetDirectoryName(mediaFileOrFolder);
    }

    viewModel.PlayNewFileOrFolder(mediaFileOrFolder);
  }
  static string firstExistingFileFromSkyCache()
  {
    var fis = new List<FileInfo>(); // foreach (var filename in Directory.GetFiles(OneDrive.AlexsVpdbFolder))				fis.Add(new FileInfo(filename));
    Array.ForEach(Directory.GetFiles(OneDrive.VpdbFolder), f => fis.Add(new FileInfo(f))); // Directory.GetFiles(OneDrive.AlexsVpdbFolder).ToList().ForEach(f => fis.Add(new FileInfo(f)));

    //fis.OrderByDescending(fi => fi.LastWriteTime).ToList().ForEach(fi => Debug.Write(string.Format("{0} - {1}\n", fi.LastWriteTime, fi.Name)));

    foreach (var fi in fis.OrderByDescending(fi => fi.LastWriteTime))
    {
      var mu = MediaUnit.LoadMetaData(fi.FullName);
      if (File.Exists(mu.PathFileCur))
        return mu.PathFileCur;
      else
        Debug.WriteLine($"::>> !exists: {mu.PathFileCur,-64}  Next..");
    }

    return @"C:\1\v\13.mpv";
  }
}
