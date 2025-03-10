using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Umbraco.Core.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(); // 0310 �[��
// Add services to the container.
//builder.Services.AddControllersWithViews();

//builder.Services.AddHttpContextAccessor();
//builder.Services.AddTransient<IUserRepository, IUserRepository>();
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie();

builder.Services.AddDbContext<HotPotContext>(
options => options.UseSqlServer(builder.Configuration.GetConnectionString("HotPotConnstring")));

//// Add services to the container. �ϥ� cookie �P�_�O�_�O�n�J���A
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddCookie(options =>
   {
       options.ExpireTimeSpan = TimeSpan.FromSeconds(20); //�L���ɶ���20����(��)
       options.SlidingExpiration = true; //�p�G�n�J�����ϥΪ̦�����(�Ҧp�o�e�ШD),�h���s�p��L���ɶ�
       options.LoginPath = "/Customers/Member_Login"; //���n�J�۰ʾɦܳo�Ӻ��}

       // �U���o�ǬO���F�����I���ШD�i�H�ǻ�Cookie
       options.Cookie.HttpOnly = true; // �O�@Cookie�קK�QJavaScript�s��
       options.Cookie.SameSite = SameSiteMode.Lax; // �]�wSameSite�����A�קK���I���D
   });
//builder.Services.AddMvc(options =>
//{
//    options.Filters.Add(new AuthorizeFilter());//�����ʧ@�����q�L�n�J���Ҥ~��ϥ�
//});
// �[�J���v
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // �}�o������ܸԲӿ��~��T
}
else
{
    app.UseExceptionHandler("/Home/Error"); // �������ҡA��V���~����
    app.UseHsts(); // �ҥ� HSTS�A�����w����
}




app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 0310  ������
app.UseAuthorization(); // �A���v


app.MapRazorPages(); // 0310 
//app.MapDefaultControllerRoute(); // 0310 // �o�ӷ|�]�m `{controller=Home}/{action=Index}/{id?}`
/*�ѨM��k �p�G�A�Ʊ� /Customers/Member_Login �O�w�]�����A�R�� MapDefaultControllerRoute();�A�O�d MapControllerRoute()�C
�Ϊ̡A�O�� MapDefaultControllerRoute();�A������ HomeController �� Index() �欰�A�������w�V�� Customers/Member_Login�C*/
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customers}/{action=Member_Login}/{id?}");
// _Member  Register    Member_Login    create    Index_Member    Index
app.Run();
