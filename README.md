# WannaSmile
  Proof of Concept Ransomware Trojan

# Important!
  Test this malware only on a virtual machine. Even if the decryption password is saved as plaintext locally, 
it doesn't mean you have to run it on your machine.
In case you somehow lost decryption password, you can find it in the following file:
"%AppData%\\wns_data.wns" (String from the 3rd line)

# Description
  This is a proof of concept ransomware malware that runs like a trojan and encrypts current user's files.
It doesn't need any privileged rights and it's undetected by some antivirus software. (Tested with Windows Defender).
The file was not uploaded on multi-engine scanning web services to maintain it's ud status as a proof.

![image](https://user-images.githubusercontent.com/25134231/111777743-5965b900-88bc-11eb-8b95-0533b842f091.png)

# Conclusion
  This malware shows us the importance of sandboxed malware analysis. If this component is missing, then some malwares
could be allowed to be executed even if the scan gave negative result. This happens due to the fact that files are scanned only
based on their signatures or integrity value. After a while the file is scanned behaviourally but it doesn't cover the cases
when it is executed for the first time. Some of antivirus engines send the unknown files to a sandbox environment but it takes a while 
for the scan report the be received, so the main goal would be speeding up this process to achieve better security and user experience.
