namespace ProxyStation.Tests.HttpTrigger
{
    class Fixtures
    {
        public readonly static string SurgeProfile1 = @"
[General]
loglevel = notify
bypass-system = true
skip-proxy = 127.0.0.0/24,192.168.0.0/16,10.0.0.0/8,172.16.0.0/12,100.64.0.0/10,17.0.0.0/8,localhost,*.local,169.254.0.0/16,224.0.0.0/4,240.0.0.0/4
bypass-tun = 127.0.0.0/24,192.168.0.0/16,10.0.0.0/8,172.16.0.0/12,100.64.0.0/10,17.0.0.0/8,localhost,*.local,169.254.0.0/16,224.0.0.0/4,240.0.0.0/4
dns-server = system, 114.114.114.114, 119.29.29.29, 223.5.5.5, 8.8.8.8, 8.8.4.4
replica = false
enhanced-mode-by-rule = true
ipv6 = false
exclude-simple-hostnames = true
test-timeout = 5
allow-wifi-access = false

[Replica]
hide-apple-request=true

[Proxy]
sadfasd= = ss, 12381293, 123, encrypt-method=aes-128-gcm, password=1231341, obfs=http, obfs-host=2341324124, udp-relay=true, interface=asdasd, allow-other-interface=true, tfo=true
香港高级 BGP 中继 5 = custom, hk5.edge.iplc.app, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
🇭🇰 中国杭州 -> 香港 01 | IPLC = ss, 123.123.123.123, 10086, encrypt-method=xchacha20-ietf-poly1305, password=gasdas, obfs=tls, obfs-host=download.windowsupdate.com, udp-relay=true
sadfasd=custom, 12381293, 123, encrypt-method=aes-128-gcm, password=1231341, obfs=http, obfs-host=2341324124, udp-relay=true, interface=asdasd, allow-other-interface=true, tfo=true

[Proxy Group]
AUTO = url-test, 香港高级 BGP 中继 1, 香港高级 BGP 中继 2, 香港高级 BGP 中继 3, 香港高级 BGP 中继 4, 香港高级 BGP 中继 5, 香港高级 BGP 中继 6, 香港高级 BGP 中继 7, 香港高级 BGP 中继 8, 香港高级 BGP 中继 9, 香港高级 BGP 中继 10, 香港高级 BGP 中继 11, 香港高级 BGP 中继 12, 香港高级 BGP 中继 13, 香港高级 BGP 中继 14, 香港高级 BGP 中继 15, 香港高级 BGP 中继 16, 香港 BGP 中继 1, 香港 BGP 中继 2, 香港 BGP 中继 3, 香港 BGP 中继 4, 香港 BGP 中继 5, 香港 BGP 中继 6, 香港 BGP 中继 7, 香港 BGP 中继 8, 香港 BGP 中继 9, 香港 BGP 中继 10, 香港 BGP 中继 11, 香港 BGP 中继 12, 香港 BGP 中继 13, 香港 BGP 中继 14, 香港 BGP 中继 15, 香港 BGP 中继 16, 香港标准 BGP 中继 1, 香港标准 BGP 中继 2, 香港标准 BGP 中继 3, 香港标准 BGP 中继 4, 香港标准 BGP 中继 5, url=http://www.gstatic.com/generate_204, interval=180, timeout=1, tolerance=100
RIXCLOUD = select, AUTO, DIRECT, 香港高级 BGP 中继 1, 香港高级 BGP 中继 2, 香港高级 BGP 中继 3, 香港高级 BGP 中继 4, 香港高级 BGP 中继 5, 香港高级 BGP 中继 6, 香港高级 BGP 中继 7, 香港高级 BGP 中继 8, 香港高级 BGP 中继 9, 香港高级 BGP 中继 10, 香港高级 BGP 中继 11, 香港高级 BGP 中继 12, 香港高级 BGP 中继 13, 香港高级 BGP 中继 14, 香港高级 BGP 中继 15, 香港高级 BGP 中继 16, 香港 BGP 中继 1, 香港 BGP 中继 2, 香港 BGP 中继 3, 香港 BGP 中继 4, 香港 BGP 中继 5, 香港 BGP 中继 6, 香港 BGP 中继 7, 香港 BGP 中继 8, 香港 BGP 中继 9, 香港 BGP 中继 10, 香港 BGP 中继 11, 香港 BGP 中继 12, 香港 BGP 中继 13, 香港 BGP 中继 14, 香港 BGP 中继 15, 香港 BGP 中继 16, 香港标准 BGP 中继 1, 香港标准 BGP 中继 2, 香港标准 BGP 中继 3, 香港标准 BGP 中继 4, 香港标准 BGP 中继 5

[Rule]
GEOIP,CN,DIRECT
FINAL,RIXCLOUD,dns-failed

";

        public readonly static string SurgeListProfile1 = @"sadfasd= = ss, 12381293, 123, encrypt-method=aes-128-gcm, password=1231341, obfs=http, obfs-host=2341324124, udp-relay=true
香港高级 BGP 中继 5 = ss, hk5.edge.iplc.app, 155, encrypt-method=rc4-md5, password=asdads, udp-relay=true
🇭🇰 中国杭州 -> 香港 01 | IPLC = ss, 123.123.123.123, 10086, encrypt-method=xchacha20-ietf-poly1305, password=gasdas, obfs=tls, obfs-host=download.windowsupdate.com, udp-relay=true
sadfasd = ss, 12381293, 123, encrypt-method=aes-128-gcm, password=1231341, obfs=http, obfs-host=2341324124, udp-relay=true
";

        public readonly static string ClashTemplate1 = @"port: 7890
socks-port: 7891
allow-lan: true
bind-address: '*'
mode: Rule
log-level: info
external-controller: 127.0.0.1:9090

Proxy Group:
- name: Proxy
  type: url-test
  url: http://www.gstatic.com/generate_204
  interval: 300
  proxies: []
- name: Default
  type: select
  proxies:
  - Proxy
  - DIRECT
- name: AdBlock
  type: select
  proxies:
  - REJECT
  - DIRECT
  - Proxy

Rule:
- GEOIP,CN,DIRECT
- MATCH,Default
- FINAL,Default
";

        public readonly static string CustomClashProfile1 = @"port: 7890
socks-port: 7891
allow-lan: true
bind-address: '*'
mode: Rule
log-level: info
external-controller: 127.0.0.1:9090
Proxy Group:
- name: Proxy
  type: url-test
  url: http://www.gstatic.com/generate_204
  interval: 300
  proxies:
  - sadfasd=
  - 香港高级 BGP 中继 5
  - ""\U0001F1ED\U0001F1F0 中国杭州 -> 香港 01 | IPLC""
  - sadfasd
- name: Default
  type: select
  proxies:
  - Proxy
  - DIRECT
- name: AdBlock
  type: select
  proxies:
  - REJECT
  - DIRECT
  - Proxy
Rule:
- GEOIP,CN,DIRECT
- MATCH,Default
- FINAL,Default
Proxy:
- name: sadfasd=
  type: ss
  server: 12381293
  port: 123
  cipher: aes-128-gcm
  password: 1231341
  udp: true
  plugin: obfs
  plugin-opts:
    mode: http
    host: 2341324124
- name: 香港高级 BGP 中继 5
  type: ss
  server: hk5.edge.iplc.app
  port: 155
  cipher: rc4-md5
  password: asdads
  udp: true
- name: ""\U0001F1ED\U0001F1F0 中国杭州 -> 香港 01 | IPLC""
  type: ss
  server: 123.123.123.123
  port: 10086
  cipher: xchacha20-ietf-poly1305
  password: gasdas
  udp: true
  plugin: obfs
  plugin-opts:
    mode: tls
- name: sadfasd
  type: ss
  server: 12381293
  port: 123
  cipher: aes-128-gcm
  password: 1231341
  udp: true
  plugin: obfs
  plugin-opts:
    mode: http
    host: 2341324124
";

        public readonly static string SurgeTemplate1 = @"[General]
interface = 0.0.0.0
socks-interface = 0.0.0.0
port = 8888
socks-port = 8889

[Proxy]
{% SERVER_LIST %}

[Proxy Group]
Default = select, Proxy, DIRECT
Proxy = url-test, {% SERVER_NAMES %}, url=http://captive.apple.com, interval=600, tolerance=200

[Rule]
RULE-SET,LAN,DIRECT
GEOIP,CN,DIRECT
FINAL,Default,dns-failed
";

        public readonly static string CustomSurgeProfile1 = @"#!MANAGED-CONFIG :// interval=43200
[General]
interface = 0.0.0.0
socks-interface = 0.0.0.0
port = 8888
socks-port = 8889

[Proxy]
sadfasd= = ss, 12381293, 123, encrypt-method=aes-128-gcm, password=1231341, obfs=http, obfs-host=2341324124, udp-relay=true
香港高级 BGP 中继 5 = ss, hk5.edge.iplc.app, 155, encrypt-method=rc4-md5, password=asdads, udp-relay=true
🇭🇰 中国杭州 -> 香港 01 | IPLC = ss, 123.123.123.123, 10086, encrypt-method=xchacha20-ietf-poly1305, password=gasdas, obfs=tls, obfs-host=download.windowsupdate.com, udp-relay=true
sadfasd = ss, 12381293, 123, encrypt-method=aes-128-gcm, password=1231341, obfs=http, obfs-host=2341324124, udp-relay=true


[Proxy Group]
Default = select, Proxy, DIRECT
Proxy = url-test, sadfasd=, 香港高级 BGP 中继 5, 🇭🇰 中国杭州 -> 香港 01 | IPLC, sadfasd, url=http://captive.apple.com, interval=600, tolerance=200

[Rule]
RULE-SET,LAN,DIRECT
GEOIP,CN,DIRECT
FINAL,Default,dns-failed
";


        public readonly static string SurgeProfile2 = @"
[General]
loglevel = notify
bypass-system = true
skip-proxy = 127.0.0.0/24,192.168.0.0/16,10.0.0.0/8,172.16.0.0/12,100.64.0.0/10,17.0.0.0/8,localhost,*.local,169.254.0.0/16,224.0.0.0/4,240.0.0.0/4
bypass-tun = 127.0.0.0/24,192.168.0.0/16,10.0.0.0/8,172.16.0.0/12,100.64.0.0/10,17.0.0.0/8,localhost,*.local,169.254.0.0/16,224.0.0.0/4,240.0.0.0/4
dns-server = system, 114.114.114.114, 119.29.29.29, 223.5.5.5, 8.8.8.8, 8.8.4.4
replica = false
enhanced-mode-by-rule = true
ipv6 = false
exclude-simple-hostnames = true
test-timeout = 5
allow-wifi-access = false

[Replica]
hide-apple-request=true

[Proxy]
香港 BGP 1 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
香港 BGP 2 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
香港 BGP 中继 1 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
香港 BGP 中继 2 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
香港高级 BGP 中继 1 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
香港高级 BGP 中继 2 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
日本 BGP 1 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
日本 BGP 2 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
日本 BGP 中继 1 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
日本 BGP 中继 2 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
日本高级 BGP 中继 1 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true
日本高级 BGP 中继 2 = custom, best-server.com, 155, rc4-md5, asdads, http://example.com/, udp-relay=true

[Proxy Group]
AUTO = url-test, 香港高级 BGP 中继 1, 香港高级 BGP 中继 2, 香港高级 BGP 中继 3, 香港高级 BGP 中继 4, 香港高级 BGP 中继 5, 香港高级 BGP 中继 6, 香港高级 BGP 中继 7, 香港高级 BGP 中继 8, 香港高级 BGP 中继 9, 香港高级 BGP 中继 10, 香港高级 BGP 中继 11, 香港高级 BGP 中继 12, 香港高级 BGP 中继 13, 香港高级 BGP 中继 14, 香港高级 BGP 中继 15, 香港高级 BGP 中继 16, 香港 BGP 中继 1, 香港 BGP 中继 2, 香港 BGP 中继 3, 香港 BGP 中继 4, 香港 BGP 中继 5, 香港 BGP 中继 6, 香港 BGP 中继 7, 香港 BGP 中继 8, 香港 BGP 中继 9, 香港 BGP 中继 10, 香港 BGP 中继 11, 香港 BGP 中继 12, 香港 BGP 中继 13, 香港 BGP 中继 14, 香港 BGP 中继 15, 香港 BGP 中继 16, 香港标准 BGP 中继 1, 香港标准 BGP 中继 2, 香港标准 BGP 中继 3, 香港标准 BGP 中继 4, 香港标准 BGP 中继 5, url=http://www.gstatic.com/generate_204, interval=180, timeout=1, tolerance=100
RIXCLOUD = select, AUTO, DIRECT, 香港高级 BGP 中继 1, 香港高级 BGP 中继 2, 香港高级 BGP 中继 3, 香港高级 BGP 中继 4, 香港高级 BGP 中继 5, 香港高级 BGP 中继 6, 香港高级 BGP 中继 7, 香港高级 BGP 中继 8, 香港高级 BGP 中继 9, 香港高级 BGP 中继 10, 香港高级 BGP 中继 11, 香港高级 BGP 中继 12, 香港高级 BGP 中继 13, 香港高级 BGP 中继 14, 香港高级 BGP 中继 15, 香港高级 BGP 中继 16, 香港 BGP 中继 1, 香港 BGP 中继 2, 香港 BGP 中继 3, 香港 BGP 中继 4, 香港 BGP 中继 5, 香港 BGP 中继 6, 香港 BGP 中继 7, 香港 BGP 中继 8, 香港 BGP 中继 9, 香港 BGP 中继 10, 香港 BGP 中继 11, 香港 BGP 中继 12, 香港 BGP 中继 13, 香港 BGP 中继 14, 香港 BGP 中继 15, 香港 BGP 中继 16, 香港标准 BGP 中继 1, 香港标准 BGP 中继 2, 香港标准 BGP 中继 3, 香港标准 BGP 中继 4, 香港标准 BGP 中继 5

[Rule]
GEOIP,CN,DIRECT
FINAL,RIXCLOUD,dns-failed

";

        public readonly static string SurgeListProfile2 = @"香港高级 BGP 中继 1 = ss, best-server.com, 155, encrypt-method=rc4-md5, password=asdads, udp-relay=true
香港高级 BGP 中继 2 = ss, best-server.com, 155, encrypt-method=rc4-md5, password=asdads, udp-relay=true
";
    }
}