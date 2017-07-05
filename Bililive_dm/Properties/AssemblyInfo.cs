using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// 組件的一般資訊是由下列的屬性集控制。
// 變更這些屬性的值即可修改組件的相關
// 資訊。

[assembly: AssemblyTitle("Bililive_dm")]
[assembly: AssemblyDescription("B 站弹幕姬")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("CopyLiu")]
[assembly: AssemblyProduct("Bililive_dm")]
[assembly: AssemblyCopyright("Copyright ©  2014 - 2017 CopyLiu")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 將 ComVisible 設定為 false 會使得這個組件中的型別
// 對 COM 元件而言為不可見。如果您需要從 COM 存取這個組件中
// 的型別，請在該型別上將 ComVisible 屬性設定為 true。

[assembly: ComVisible(false)]

//為了建置可當地語系化的應用程式，請設定 
//.csproj 檔案中的 <UICulture>CultureYouAreCodingWith</UICulture
//<PropertyGroup> 內部。舉例來說，如果您使用的是 US English
//將原始程式檔中的 <UICulture> 設定成 en-US。然後取消註解
//底下的 NeutralResourceLanguage 屬性。更新 "en-US"
//下面一行符合專案檔中的 UICulture 設定。

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //主題特定資源字典的位置
    //(用於當資源不在頁面、
    // 或應用程式資源字典)
    ResourceDictionaryLocation.SourceAssembly //泛用資源字典的位置
    //(用於當資源不在頁面、
    // 應用程式，或任何特定主題的資源字典)
    )]


// 組件的版本資訊是由下列四項值構成:
//
//      主要版本
//      次要版本 
//      組建編號
//      修訂編號
//
// 您可以指定所有的值，也可以依照以下的方式，使用 '*' 將組建和修訂編號
// 指定為預設值:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.1.*")]
[assembly: AssemblyFileVersion("1.1.*")]