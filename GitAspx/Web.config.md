<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="RepositoriesDirectory" value="D:\Data\git"/>
    <add key="ReceivePack" value="true"/>
    <add key="UploadPack" value="true"/>
    <add key="logdir" value="C:\work_exe\logs"/>
    <add key="redirect.aspx" value="0" />

    <add key="ClientValidationEnabled" value="false"/>
    <add key="UnobtrusiveJavaScriptEnabled" value="false"/>
  </appSettings>
  <system.web>
    <customErrors mode="Off"/>
    <trace enabled="true" localOnly="false"/>
    <compilation debug="true" targetFramework="4.5">
      <assemblies>
        <add assembly="System.Web.Abstractions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
        <add assembly="System.Web.Routing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
        <add assembly="System.Web.Mvc, Version=5.2.3.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"/>
        <add assembly="System.Web.WebPages, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
        <add assembly="System.Web.Razor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
      </assemblies>
    </compilation>
    <httpRuntime executionTimeout="10000" maxRequestLength="30048576"
	requestValidationMode="2.0" 
        requestPathInvalidCharacters="&lt;,&gt;,*" />

    <authentication mode="None">
      <forms loginUrl="~/Account/LogOn" timeout="2880" />
    </authentication>
    <pages controlRenderingCompatibilityVersion="4.0">
      <namespaces>
        <add namespace="System.Web.Mvc"/>
        <add namespace="System.Web.Mvc.Ajax"/>
        <add namespace="System.Web.Mvc.Html"/>
        <add namespace="System.Web.Routing"/>
        <add namespace="System.Web.Helpers"/>
        <add namespace="System.Web.WebPages"/>
        <add namespace="GitAspx" />
        <add namespace="GitAspx.ViewModels" />
        <add namespace="GitAspx.Properties" />
      </namespaces>
    </pages>
  </system.web>
  <system.webServer>
    <validation validateIntegratedModeConfiguration="false"/>
    <security>
      <requestFiltering >
        <fileExtensions allowUnlisted="true">
          <add allowed="true" fileExtension="cs" />
        </fileExtensions>
      </requestFiltering>
    </security>
    <modules runAllManagedModulesForAllRequests="true">
      <remove name="SXHttpHandler"/>
      <add name="SXHttpHandler" type="AiLib.Web.HttpHandler, MvcHttp"/>
    </modules>
    <handlers>
      <remove name="ExtensionlessUrlHandler-ISAPI-4.0_32bit" />
      <add name="ExtensionlessUrlHandler-ISAPI-4.0_32bit" path="*."
           verb="GET,HEAD,POST,CONNECT,DEBUG,PUT,DELETE,PATCH,OPTIONS"
           modules="IsapiModule"
           scriptProcessor="%windir%\Microsoft.NET\Framework\v4.0.30319\aspnet_isapi.dll"
           preCondition="classicMode,runtimeVersionv4.0,bitness32" responseBufferLimit="0" />

      <remove name="ExtensionlessUrlHandler-Integrated-4.0" />
      <add name="ExtensionlessUrlHandler-Integrated-4.0" path="*."
           verb="GET,HEAD,POST,CONNECT,DEBUG,PUT,DELETE,PATCH,OPTIONS"
           type="System.Web.Handlers.TransferRequestHandler"
           preCondition="integratedMode,runtimeVersionv4.0" />
    </handlers>
  </system.webServer>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Web.Mvc" publicKeyToken="31BF3856AD364E35" culture="neutral"/>
        <bindingRedirect oldVersion="0.0.0.0-5.2.3.0" newVersion="5.2.3.0"/>
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>