### Dropbox Uploader

Dropbox Uploader is a Unity Component that can send Experimental Results directly to your Dropbox. Dropbox Uploader works on the Desktop and in WebGL.

#### Getting Started

1 - Copy the *DropboxUploader* folder into your project, and add the `DropboxUploader` Component to a GameObject. You then need to authorise `DropboxUploader`.

2 - Click *Open App Console* in the Inspector to open the [Dropbox App Console](https://www.dropbox.com/developers/apps). 

Create an App for your experiment here, if you have not yet done so.

The 'App' you are creating is for your experiment (there is no *DropboxUploader App* - DropboxUploader is a tool to upload to a specific App). Set the App folder name here too. The folder the App can write to will be in `/Apps/[App folder name]`

Make sure the app has the `files.content.write` permission.

3 - Enter the App key and App secret into Unity and click the Authorise button. A new browser tab will open asking you to provide authorisation for your new App.

4 - At the end of this process will be an Access code. Enter the Access code into the Code field and click Accept.

5 - After a few moments, DropboxUploader will get a token and store it on the Component in the Scene.


You can now use the `UploadCoroutine` method to write a byte array to a file with the specified name.


#### Things to consider

* The Component will store the App Key, App Secret and Refresh Token in the .unity scene file. Do not commit this to a public repository as it can be used to perform actions against your account.

* Do not use DropboxUploader to upload Personally Identifiable Information about participants to Dropbox, unless you have agreed with your data protection officer that the use of Dropbox in your experiment is compliant with the GDPR.


