namespace ProxyStation.ProfileParser.Template
{
    public static class Surge
    {
        public const string ServerListPlaceholder = "{% SERVER_LIST %}";

        public const string ServerNamesPlaceholder = "{% SERVER_NAMES %}";

        public readonly static string Template = $@"[General]
loglevel = notify
dns-server = system, 223.5.5.5, 223.6.6.6, 8.8.8.8, 8.8.4.4
skip-proxy = 127.0.0.1, 192.168.0.0/16, 10.0.0.0/8, 172.16.0.0/12, 100.64.0.0/10, 17.0.0.0/8, localhost, *.local, *.crashlytics.com
external-controller-access = best-proxy-staion@0.0.0.0:6170
allow-wifi-access = true
interface = 0.0.0.0
socks-interface = 0.0.0.0
port = 8888
socks-port = 8889
enhanced-mode-by-rule = false
show-error-page-for-reject = true
exclude-simple-hostnames = true
ipv6 = true
replica = false
network-framework = true

[Replica]
hide-apple-request = true
hide-crashlytics-request = true
hide-udp = false
use-keyword-filter = false

[Proxy]
{Surge.ServerListPlaceholder}

[Proxy Group]
Global Traffic = select, Auto Proxy, DIRECT
CN Traffic = select, DIRECT, Auto Proxy
Default = select, Auto Proxy, DIRECT
AdBlock = select, REJECT, REJECT-TINYGIF, Global Traffic, CN Traffic
Auto Proxy = url-test, {Surge.ServerNamesPlaceholder}, url=http://captive.apple.com, interval=600, tolerance=200 

[Rule]
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/Anti-GFW%2B.list,Global Traffic
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/Anti-GFW.list,Global Traffic
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/Stream.list,Global Traffic
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/Apple.list,Global Traffic
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/Reject.list, AdBlock
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/Reject-GIF.list, AdBlock
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/China.list,CN Traffic
RULE-SET,https://raw.githubusercontent.com/rixCloud-Inc/rixCloud-Surge3_Rules/master/Netease_Music.list,CN Traffic
GEOIP,CN,CN Traffic
FINAL,Default,dns-failed
";
    }

}