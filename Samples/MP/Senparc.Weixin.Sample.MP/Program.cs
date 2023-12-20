var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//ʹ�ñ��ػ���������
builder.Services.AddMemoryCache();

#region ���΢�����ã�һ�д��룩

//Senparc.Weixin ע�ᣨ���룩
builder.Services.AddSenparcWeixinServices(builder.Configuration);

#endregion

var app = builder.Build();

#region ����΢�����ã�һ����룩

//�ֶ���ȡ������Ϣ��ʹ�����·���
//var senparcWeixinSetting = app.Services.GetService<IOptions<SenparcWeixinSetting>>()!.Value;

//����΢�����ã����룩
var registerService = app.UseSenparcWeixin(app.Environment,
    null /* ��Ϊ null �򸲸� appsettings  �е� SenpacSetting ����*/,
    null /* ��Ϊ null �򸲸� appsettings  �е� SenpacWeixinSetting ����*/,
    register => { /* CO2NET ȫ������ */ },
    (register, weixinSetting) =>
{
    //ע�ṫ�ں���Ϣ������ִ�ж�Σ�ע�������ںţ�
    register.RegisterMpAccount(weixinSetting, "��ʢ������С���֡����ں�");
});

#region ʹ�� MessageHadler �м��������ȡ������������ Controller

//MessageHandler �м�����ܣ�https://www.cnblogs.com/szw/p/Wechat-MessageHandler-Middleware.html
//ʹ�ù��ںŵ� MessageHandler �м����������Ҫ���� Controller��
app.UseMessageHandlerForMp("/WeixinAsync", CustomMessageHandler.GenerateMessageHandler, options =>
{
    //��ȡĬ��΢������
    var weixinSetting = Senparc.Weixin.Config.SenparcWeixinSetting;

    //[����] ����΢������
    options.AccountSettingFunc = context => weixinSetting;

    //[��ѡ] ��������ı����Ȼظ����ƣ����������ÿͷ��ӿڷ����λظ���
    options.TextResponseLimitOptions = new TextResponseLimitOptions(2048, weixinSetting.WeixinAppId);
});

#endregion

#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

#region �˲��ִ���Ϊ Sample �����ļ���Ҫ����ӣ�ʵ����Ŀ�������
#if DEBUG
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "wwwroot"),
//    RequestPath = new PathString("")
//});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"..", "..", "Shared", "Senparc.Weixin.Sample.Shared", "wwwroot")),
    RequestPath = new PathString("")
});
#endif
#endregion

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
