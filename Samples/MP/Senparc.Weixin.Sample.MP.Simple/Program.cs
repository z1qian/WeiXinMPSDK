using System.Text;
using Senparc.Weixin.MP.AdvancedAPIs.MerChant;

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

//����΢�����ã����룩
var registerService = app.UseSenparcWeixin(app.Environment,
        senparcSetting: null /* ��Ϊ null �򸲸� appsettings  �е� SenpacSetting ����*/,
        senparcWeixinSetting: null /* ��Ϊ null �򸲸� appsettings  �е� SenpacWeixinSetting ����*/,
        globalRegisterConfigure: register => { /* CO2NET ȫ������ */ },
        weixinRegisterConfigure: (register, weixinSetting) => {/* ע�ṫ�ںŻ�����ƽ̨��Ϣ������ִ�ж�Σ�ע�������ںţ�*/},
        autoRegisterAllPlatforms: true /* �Զ�ע������ƽ̨ */
        );

#region ʹ�� MessageHadler �м��������ȡ������������ Controller

//MessageHandler �м�����ܣ�https://www.cnblogs.com/szw/p/Wechat-MessageHandler-Middleware.html
//ʹ�ù��ںŵ� MessageHandler �м����������Ҫ���� Controller��
app.UseMessageHandlerForMp("/WeixinAsync", CustomMessageHandler.GenerateMessageHandler, options =>
{
    //��ȡĬ��΢������
    var weixinSetting = Senparc.Weixin.Config.SenparcWeixinSetting.Items["OpenVip"];

    //[����] ����΢������
    options.AccountSettingFunc = context => weixinSetting;

    //[��ѡ] ��������ı����Ȼظ����ƣ����������ÿͷ��ӿڷ����λظ���
    options.TextResponseLimitOptions = new TextResponseLimitOptions(2048, weixinSetting.WeixinAppId);
});

#endregion

#endregion

#region �߼��ӿڵ���ʾ��

app.MapGroup("/").MapGet("/TryApi", async () =>
{
    //��ʾ��ȡ�ѹ�ע�û��� OpenId��������ȡ�ĵ�һ����

    var weixinSetting = Senparc.Weixin.Config.SenparcWeixinSetting.MpSetting;
    var users = await Senparc.Weixin.MP.AdvancedAPIs.UserApi.GetAsync(weixinSetting.WeixinAppId, null);

    Console.WriteLine($"չʾǰ {users.count} �� OpenId");

    return users.data.openid;
});

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
