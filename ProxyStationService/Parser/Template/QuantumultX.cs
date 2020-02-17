namespace ProxyStation.ProfileParser.Template
{
    public static class QuantumultX
    {
        public const string ServerListUrlPlaceholder = "{% SERVER_LIST_URL %}";

        public readonly static string Template = $@"[general]
excluded_routes = 192.168.0.0/16, 10.0.0.0/8, 172.16.0.0/12, 100.64.0.0/10, 17.0.0.0/8
network_check_url = http://www.gstatic.com/generate_204
server_check_url = http://www.gstatic.cn/generate_204
geo_location_checker = http://ip-api.com/json/?lang=zh-CN, https://github.com/KOP-XIAO/QuantumultX/raw/master/Scripts/IP_API.js

[dns]
server = 119.29.29.29
server = 119.28.28.28
server = 1.2.4.8
server = 182.254.116.116

[policy]
static=ADBlock, reject, direct, Auto Proxy
static=CN Traffic, direct, Auto Proxy
static=Global Traffic, Auto Proxy, direct, Proxy
static=Default, Global Traffic, CN Traffic

[server_remote]
{ServerListUrlPlaceholder}, as-policy=available, tag=Auto Proxy

[filter_remote]
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/AdBlock.list, tag=AdBlock, force-policy=ADBlock
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Netflix.list, tag=Netflix, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/YouTube.list, tag=YouTube, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Microsoft.list, tag=Microsoft, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/GMedia.list, tag=GMedia, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/PayPal.list, tag=PayPal, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Speedtest.list, tag=Speedtest, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Telegram.list, tag=Telegram, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Outside.list, tag=Outside, force-policy=Global Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/CMedia.list, tag=CMedia, force-policy=CN Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Apple.list, tag=Apple, force-policy=CN Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Mainland.list, tag=Mainland, force-policy=CN Traffic
https://raw.githubusercontent.com/GeQ1an/Rules/master/QuantumultX/Filter/Netease%20Music.list, tag=Netease Music, force-policy=CN Traffic

[filter_local]
host-suffix, local, direct
ip-cidr, 10.0.0.0/8, direct
ip-cidr, 17.0.0.0/8, direct
ip-cidr, 100.64.0.0/10, direct
ip-cidr, 127.0.0.0/8, direct
ip-cidr, 172.16.0.0/12, direct
ip-cidr, 192.168.0.0/16, direct
geoip, cn, CN Traffic
final, Default

[rewrite_remote]
https://raw.githubusercontent.com/ConnersHua/Profiles/master/Quantumult/X/Rewrite.conf
http://cloudcompute.lbyczf.com/quanx-rewrite
";
    }

}