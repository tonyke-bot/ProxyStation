# ProxyStation
A proxy servers list management program runs on Azure Functions. Support most of shadowsocks client including Surge, Clash, Shadowrocket, Surfboard.

### Demo
* General Profile: <https://proxy-station.azurewebsites.net/api/train/demo/general>
* Surge Profile: <https://proxy-station.azurewebsites.net/api/train/demo/surge>
* Surge List: <https://proxy-station.azurewebsites.net/api/train/demo/surge-list>
* Clash Profile: <https://proxy-station.azurewebsites.net/api/train/demo/clash>
* Surfboard Profile: <https://proxy-station.azurewebsites.net/api/train/demo/surfboard>

### App Supported
| Alias        | App                                                | As source | As output |
|:------------:|:--------------------------------------------------:|:---------:|:---------:|
| `general`    | Shadowrocket<br>Shadowsocks<br>ShadowsocksR<br>... | √         | √         |
| `surge`      | Surge                                              | √         | √         |
| `surge-list` | Surge(Proxy List)                                  | √         | √         |
| `clash`      | Clash                                              | √         | √         |
| `surfboard`  | Surfboard                                          | X         | √         |

* `Surfboard` as input is not supported becasue it's still in beta stage

### How to Deploy to Azure Functions
Coming soon.

### How to Add Profile
This function loads profile from environment variables.
In production, the variables can be set in *Azure Portal*.
During local testing, the variables can be write into `local.settings.json`.
The key name of the variable is profile name, the value is a structured JSON string.
```json
// Please write in one line. This snippet is just an example
{
    "name": "<string, name of profile>",
    "source": "<string, the url to source profile>",
    "type": "<string, the type of source profile, available values seen `alias`>",
    "allowDirectAccess:": "<boolean, if true and when target and source type are the same, the function will return un-processed profile>",
}
```


### API

#### Get Built-in Profile
`GET /api/train/{profile-name}/{output}`

Description:
Developer can pre-define some profile sources before deploy to *Azure Functions*.
Invoking this API will cause function to retrive profile from source accordingly,
parse the source and format to specific format according to argument `output`.

Argument:
* `profile-name` is pre-defined name of profile source
* `output` is the type of target profile
