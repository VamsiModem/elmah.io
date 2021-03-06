elmah.io
======

[![Build status](http://teamcity.codebetter.com/app/rest/builds/buildType:ElmahIo_ElmahIoV2/statusIcon)](http://teamcity.codebetter.com/viewType.html?buildTypeId=ElmahIo_ElmahIoV2)
[![NuGet](https://img.shields.io/nuget/vpre/elmah.io.svg)](https://www.nuget.org/packages/elmah.io)

ELMAH error logger for sending errors to elmah.io.

Getting started
-----------------
To start logging errors from your web application to elmah.io, add the [elmah.io NuGet](http://www.nuget.org/packages/elmah.io/) package. During the installation, a dialog will prompt you for your log id. The log id can be found on the first tab of your log settings.

Manul configuration
-----------------------
If you want to configure ELMAH and elmah.io yourself, you should install the [elmah.io.core NuGet](http://www.nuget.org/packages/elmah.io.core/) package. This package contains the core elmah.io assemmblies, but not config. Add the following code to your web.config (you may need to merge some of the elements with existing elements):

```xml
<configuration>
  <configSections>
    <sectionGroup name="elmah">
      <section name="security" requirePermission="false" type="Elmah.SecuritySectionHandler, Elmah" />
      <section name="errorLog" requirePermission="false" type="Elmah.ErrorLogSectionHandler, Elmah" />
      <section name="errorMail" requirePermission="false" type="Elmah.ErrorMailSectionHandler, Elmah" />
      <section name="errorFilter" requirePermission="false" type="Elmah.ErrorFilterSectionHandler, Elmah" />
    </sectionGroup>
  </configSections>
  <system.web>
    <httpModules>
      <add name="ErrorLog" type="Elmah.ErrorLogModule, Elmah" />
      <add name="ErrorMail" type="Elmah.ErrorMailModule, Elmah" />
      <add name="ErrorFilter" type="Elmah.ErrorFilterModule, Elmah"/>
    </httpModules>
  </system.web>
  <system.webServer>
    <validation validateIntegratedModeConfiguration="false" />
    <modules>
      <add name="ErrorLog" type="Elmah.ErrorLogModule, Elmah" preCondition="managedHandler" />
      <add name="ErrorMail" type="Elmah.ErrorMailModule, Elmah" preCondition="managedHandler" />
      <add name="ErrorFilter" type="Elmah.ErrorFilterModule, Elmah" preCondition="managedHandler" />
    </modules>
  </system.webServer>
  <elmah>
    <errorLog type="Elmah.Io.ErrorLog, Elmah.Io" LogId="ELMAHIOLOGID" />
    <security allowRemoteAccess="false" />
  </elmah>
  <location path="elmah.axd" inheritInChildApplications="false">
    <system.web>
      <httpHandlers>
        <add verb="POST,GET,HEAD" 
             path="elmah.axd" 
             type="Elmah.ErrorLogPageFactory, Elmah" />
      </httpHandlers>
      <!-- 
        See http://code.google.com/p/elmah/wiki/SecuringErrorLogPages for 
        more information on using ASP.NET authorization securing ELMAH.

      <authorization>
        <allow roles="admin" />
        <deny users="*" />  
      </authorization>
      -->  
    </system.web>
    <system.webServer>
      <handlers>
        <add name="ELMAH"
             verb="POST,GET,HEAD"
             path="elmah.axd" 
             type="Elmah.ErrorLogPageFactory, Elmah"
             preCondition="integratedMode" />
      </handlers>
    </system.webServer>
  </location>
</configuration>
```

Replace ELMAHIOLOGID with your log id, found beneath the log settings.

Logging custom errors from a web application
---------------------------------------------------
ELMAH and elmah.io automatically logs all of your websites unhandled exceptions. Sometimes you may want to manually log errors to elmah.io, without returning a 500 error response to the client. This can be achieved by using ELMAH's ErrorSignal class:

```c#
try 
{
    // business logic
}
catch(Exception e)
{
    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
}
```

This tells elmah.io to log the exception named *e*.

Logging message from other types of applications
--------------------------------------------------------
ELMAH is mainly used for logging unhandled exceptions from .NET web applications, but nothing inside the engine room of ELMAH is web dependant. elmah.io extends this even further by supporting severities and non-exception log messages. To integrate with elmah.io from a log framework og log custom messages, install the [elmah.io.client](http://www.nuget.org/packages/elmah.io.client) package. The client package isn't dependant of neither System.Web nor ELMAH.

To log messages to elmah.io, through the elmah.io.client, start by creating a new Logger:

```c#
var logger = Logger.Create(new Guid("ELMAHIOLOGID"));
```

Replace ELMAHIOLOGID with your log id. To start logging messages, use on of the log methods:

```c#
void Verbose(string messageTemplate, params object[] propertyValues);
void Verbose(Exception exception, string messageTemplate, params object[] propertyValues);
void Debug(string messageTemplate, params object[] propertyValues);
void Debug(Exception exception, string messageTemplate, params object[] propertyValues);
void Information(string messageTemplate, params object[] propertyValues);
void Information(Exception exception, string messageTemplate, params object[] propertyValues);
void Warning(string messageTemplate, params object[] propertyValues);
void Warning(Exception exception, string messageTemplate, params object[] propertyValues);
void Error(string messageTemplate, params object[] propertyValues);
void Error(Exception exception, string messageTemplate, params object[] propertyValues);
void Fatal(string messageTemplate, params object[] propertyValues);
void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);
```

Severities follow the pattern found in other logging frameworks for .NET like [Serilog](http://serilog.net/).

We also provide a raw message based API, which receives a [Message](https://github.com/elmahio/elmah.io/blob/apiv2/Elmah.Io.Client/Message.cs):

```c#
string Log(Message message);
IAsyncResult BeginLog(Message message, AsyncCallback asyncCallback, object asyncState);
```

The *Message* object contains a lot of properties like Title, Severity, Details and much more.
