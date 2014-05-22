QasProofOfConcept


The Address Type ahead product is exposed by QAS as a SOAP web service.
To hit it we need access to the public internet.
Our Citrix environment has been set up with a new proxy server, one that will have to be configured in UAT and Prod. It’s a Symantec product which seems to do a bit of trickery.
This page: http://ip.symantec.com/ echos back the IP and hostname you have presented (like whatismyip.com) 
I’ve noticed that the host displayed actually changes, the Symantec product is some form of cloud offering, and the actual proxy server that takes your request can change (load balancing or something).  E.g. proxy40.messagelabs.net, proxy58.messagelabs.net
Sometimes CECC can authenticate just fine (i.e. it can access the web service).
Other times it gets a 407 (proxy authentication required).
We can’t seem to work out a pattern to it yet, but I am worried that some of the Symantec hosts might have different authentication requirements.
I’ve asked our infra team to see if they can ask Symantec to assist.

The UAT environment has been set up with the Symantec proxy.
You can set your local machine to use it with the following settings:
pac.au.imckesson.com,  port 8080.

If you could help Thara by looking at the way we authenticate to the proxy and trying to see if there’s any potential flaw in what’s being done it would be great.

If we also start to get any defects from Mike’s hardening, between Thara and yourself I’m sure you can work through them.

