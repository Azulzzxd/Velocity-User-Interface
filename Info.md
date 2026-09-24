<table align="center">
<tr>
<td align="center" style="background-color:#fff3cd; border:1px solid #ffe69c; border-radius:12px; padding:16px;">

<strong>Gladly use this source-repo for learning NET-UI development!</strong><br>
Of course crediting the repo would be appreciated. This code is not perfect, but it *can* be used as learning-material. 

Use it wisely! 
</td>
</tr>
</table>

# Why publish?

Since this user interface was released, many parts of it have been copied using .NET decompilers like [ILSpy](https://github.com/icsharpcode/ilspy).

Because of how the [.NET framework](https://en.wikipedia.org/wiki/.NET) works, there is no reliable way to fully prevent people from recovering the source code.

> For this reason, and with permission from CG, I have decided to open source the user interface, as it will no longer be used after Velocity v3 releases.
> 
> Just a note from me for the skids: your decompile was relatively poor in quality. Your GPT prompts did not restore the Win32 logic and half of the code was like spaghetti. I also find it funny that you described the condition of the decompile as “near mint" because that was way below "good" let alone "near mint". Maybe put more effort into it next time.
>
> Love, your dearest Azul :)

# What does this include?

After the initial release of the new interface, many people provided feedback which helped me figure out what people actually wanted.

## The following was added/brought back from the old interface
* The **Client Manager** was brought back with an option to toggle it off
* **Injector checks** were added
* **Anti-Virus detector**
* A new **Script-Ware-like sidebar** has been added
* **Changelogs page**
* **New custom message boxes**
* **Notifications through the Velocity WebSocket**
* **Proper multi-instance support**
* A new **theme menu**
* **Accent colors**
* **Support for animated GIFs in the media overlay**
* A new **WinUI-like title bar effect**
