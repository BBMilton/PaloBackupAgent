

<h1 align="center">
   UNOFFICIAL Palo Backup Agent
</h1>

<p align="center">
 <img src="https://forthebadge.com/images/badges/built-with-love.svg">
 <img src="https://forthebadge.com/images/badges/for-you.svg">
</p>

<h2 align="center">
   Unofficial tools for automating backups for Palo Alto devices
</h2>

### :sparkles: Getting Started :sparkles:

1. Have a fully updated Windows machine with [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) installed (preinstalled on Server 2022, Windows 10 1903+ & Windows 11)
2. Download the latest PaloBackupAgent.exe and PaloPHashGenerator.exe from the [Releases](https://github.com/BBMilton/PaloBackupAgent/releases) page 
3. Download or create a config.xml - see example
4. On your Palo device create either a:
	-  <ins>ReadOnly</ins> Superuser (if you want passwords and private keys to be backed up)
	- Create a Role Based user with access to the XML API (cannot backup the device state, passwords, or private keys)
5. Run PaloPHashGenerator.exe and either:
    - Generate and copy the PHash then save it to the config.xml under the PHash tag
    - Generate a PHash, choose "Save to Windows Credential Manager", copy the tag then save it to the config.xml
6. Run PaloBackupAgent.exe manually and make sure it can create the specified files
7. Create a Windows Task Schedule to run PaloBackupAgent whenever and how often you like.

### :dizzy: Restoring :dizzy: 

For most Palo devices the option to import the files will be under Device > Setup > Operation > Import.

### :sparkles: Notes :sparkles:
- PaloBackupAgent.exe only requires Read/Write access to the backup path and does not need any admins access. Always run as least privilege!
- DeviceState.tgz is a compressed file and can be opened as a zip - if you can't open it or it is 1KB the user account on Palo is not a Superuser and needs to be.
- Total backup size will be around 300KB~ per device, but will be larger if you have custom webpages set. 
- Always check Palo's [knowledge base](https://knowledgebase.paloaltonetworks.com/) for best practices for your device and version
- If you run into any issues using this program, message [@BBMilton](https://www.github.com/BBMilton) or let me know over in the [Issues](https://github.com/BBMilton/PaloBackupAgent/issues) tab

#### Example config.xml

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Devices>
	<Device>
		<Hostname>firewall.domain.com</Hostname>
		<PHash>ExampleHash</PHash>
		<BackupConfig>true</BackupConfig>
		<BackupState>true</BackupState>
		<BackupVersion>true</BackupVersion>                   <!--Optional-->
		<TLS>1.2</TLS>                                        <!--Optional-->
		<BackupPath>C:\Palo Backup Agent\Backups</BackupPath> <!--Optional-->
		<LogFile>C:\Palo Backup Agent\log.txt</LogFile>       <!--Optional-->
	</Device>
	<Device>
		<Hostname>firewall2.domain.com</Hostname>
		<CredentialManager>PBA-firewall2.domain.com</CredentialManager>
		...
	</Device>
</Devices>
```