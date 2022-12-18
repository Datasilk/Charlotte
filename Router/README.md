# Charlotte's Web Router
#### A load balancer for handling requests to multiple instances of Charlotte

The router runs as a service and binds to a specific port. You can change that port from within the `appsettings.json` file.

To run in Visual Studio, make sure to set your Solution to run multiple projects so that Charlotte can run alongside Router.

### Installation
1. Build or publish the Router application
2. Within PowerShell, run `.\Router.exe` after navigating to the published folder to start the Router service.
3. Navigate to `http://localhost:7007` (the configured URL) to log into the dashboard
4. Configure any instances of Charlotte that you have running in your environment
5. Click the link 'Show Console' to display a live output of debug information from the Router & Charlotte
6. from within the Dashboard console, click the link 'Check URL' to manually send a request to Charlotte so you can test the processing of a URL within Charlotte

To use in your project, send a request using the `POST` method to `http://localhost:7007/GetDOM` with POST data (e.g. `url=https://my-test-domain.com/`).
Router will redirect the request and post data to an instance of Charlotte to extract the DOM in JSON format from the requested URL

### Collector Installation
Charlotte's Web Router relies on a separate application used to install & run Collector's infrastructure.
Follow the installation instructions for [Collector Updater](https://github.com/Datasilk/Collector-Updater) 
to install & run all of Collector's infrastructure, which includes Charlotte's Web Router.