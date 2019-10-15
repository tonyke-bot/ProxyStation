namespace ProxyStation.ProfileParser
{
    public static partial class ProfileSnippet
    {
        public const string SurgeProxyPlaceHolder = "# === PROXY SETUP PLACEHOLDER ===";
        
        public const string SurgeCommon = @"[General]
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
ipv6 = false
replica = false

[Replica]
hide-apple-request = true
hide-crashlytics-request = true
hide-udp = false
use-keyword-filter = false

"
+ SurgeProxyPlaceHolder
+ @"
[Rule]
USER-AGENT,AppStore*,Default
USER-AGENT,com.apple.appstored*,Default
USER-AGENT,TestFlight*,Default
DOMAIN-SUFFIX,apple.com,Default
DOMAIN-SUFFIX,icloud.com,Default
DOMAIN-SUFFIX,itunes.com,Default
DOMAIN-SUFFIX,me.com,Default
DOMAIN-SUFFIX,mzstatic.com,Default
RULE-SET,SYSTEM,Default
RULE-SET,https://raw.githubusercontent.com/lhie1/Rules/master/Surge3/Reject.list,AdBlock
RULE-SET,https://raw.githubusercontent.com/lhie1/Rules/master/Surge3/GlobalTV.list,Default
RULE-SET,https://raw.githubusercontent.com/lhie1/Rules/master/Surge3/Proxy.list,Default
RULE-SET,LAN,DIRECT
GEOIP,CN,DIRECT
FINAL,Default,dns-failed
";
    }

}