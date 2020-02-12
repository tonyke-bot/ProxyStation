# ProxyStation
![Build Stauts](https://github.com/THaGKI9/ProxyStation/workflows/CI%20Test/badge.svg)
[![codecov](https://codecov.io/gh/THaGKI9/ProxyStation/branch/master/graph/badge.svg)](https://codecov.io/gh/THaGKI9/ProxyStation)

A proxy servers list management program runs on Azure Functions. Support most of shadowsocks client including Surge, Clash, Shadowrocket.

### Demo
* General Profile: <https://proxy-station.azurewebsites.net/api/train/demo/general>
* Surge Profile: <https://proxy-station.azurewebsites.net/api/train/demo/surge>
* Surge List: <https://proxy-station.azurewebsites.net/api/train/demo/surge-list>
* Clash Profile: <https://proxy-station.azurewebsites.net/api/train/demo/clash>
* QuantumultX Profile: <https://proxy-station.azurewebsites.net/api/train/demo/quantumult-x>
* QuantumultX List: <https://proxy-station.azurewebsites.net/api/train/demo/quantumult-x-list>

### App Supported
| Alias               | App                                                       | As source | As output |
|:-------------------:|:---------------------------------------------------------:|:---------:|:---------:|
| `general`           | Shadowrocket<br>Shadowsocks<br>ShadowsocksR<br>Quantumult | √         | √         |
| `surge`             | Surge                                                     | √         | √         |
| `surge-list`        | Surge(Proxy List)                                         | √         | √         |
| `clash`             | Clash                                                     | √         | √         |
| `quantumult-x`      | QuantumultX                                               | √         | √         |
| `quantumult-x-list` | QuantumultX(Proxy List)                                   | √         | √         |                 

### How to Deploy to Azure Functions
Coming soon.

### How to Add Profile
This function loads profile from environment variables.
In production, the variables can be set in *Azure Portal*.
During local testing, the variables can be write into `local.settings.json`.
The key name of the variable is profile name, the value is a structured JSON string.
```jsonc
// Please write in one line. This snippet is just an example
{
    "name": "<string, name of profile>",
    "source": "<string, the url to source profile>",
    "type": "<string, the type of source profile, available values: general/surge/surge-list/clash/alias>",
    
    "allowDirectAccess:": "<boolean, if true and when target and source type are the same, the function will return un-processed profile. Ignored when type is alias>",
    "filters": [ // filters are diabled when allowDirectAccess is enabled
        {
            "name": "<string, filter name>",
            "mode": "<whitelist | blacklist(default)>",
            ... // filter options
        },
        ...
    ]
}

// There is one special case that allows you refer to another profile
{
    "name": "<string, name of profile>",
    "source": "<string, reference profile name>",
    "type": "alias"
}
```

### API

#### Get Built-in Profile
`GET /api/train/{profile-name}/{output?}?template={templateUrlOrName}`

Description:
Developer can pre-define some profile sources before deploy to *Azure Functions*.
Invoking this API will cause function to retrive profile from source accordingly,
parse the source and format to specific format according to argument `output`.

Query:
* `templateUrlOrName` is a url to custom template or pre-defined name in environment variables table. 

Argument:
* `profile-name` is pre-defined name of profile source
* `output` is the type of target profile, if omitted, will be inferred from user-agent. If `output = original` and `allowDirectAccess` is enabled for this profile, service will return original content downloading from source url. 


### Supported Filters
Filters allow you to control outcome of servers list.

#### Server Name Filter
```jsonc
{
    "name": "name",
    "keyword": "<string, keyword to match>",
    "matching": "<suffix | prefix | contains(default), imply how to match the keyword>"
}
```