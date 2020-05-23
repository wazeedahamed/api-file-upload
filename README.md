# Introduction
This is . NET web API code to upload a large file to server `App_Data` folder in 5MB chunks. When server support is limited to size of file to be uploaded in one HTTP request (e.g, 5MB per request), this code can be used as an alternative to transfer large size file to server in successive chunks. For file which is less in size than chunck size, HTTP request will be processed as usual without chunks.

## Folder Information

1. `LargeFileUpload` folder holds .NET API code.
2. `html` folder holds a simple html web application that calls Web API, and sends file in chunks when required.

## How to check?

Deploy API code in server or Run using Visual Studio or any other IDE of your preference.

Now open command prompt and navigate `html` folder content. Run the below command to install required `node_modules` . This is required to run on first time.

> npm install

Run below command to serve and open web application to upload. This will run predefined script from `package.json` file

> npm start

Check the API url on the page. Update below configuration if necessary in `html/content/index.html` page

``` javascript
var config = {
    // Web API URL
    api: 'http://localhost:55622',
    // Chunk Size in MB
    chunkSize: 5
}
```
