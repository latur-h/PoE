#Requires AutoHotkey v2.0
#SingleInstance

_windowTitle := "ahk_exe PathOfExile.exe"
_flag := false

_abilityKeys := Map("Q", "Q", "W", "W", "E", "E", "R", "R", "T", "T", "A", "A", "S", "S", "D", "D", "F", "F", "G", "G")

_flaskKeys := Map("1", "1", "2", "2", "3", "3", "4", "4", "5", "5")

_flask1 := Map("Top", Map("X", 441, "Y", 1344, "Color", ""), "Bottom", Map("X", 416, "Y", 1432, "Color", "0xF9D799"), "Time", "", "Key", "1")
_flask2 := Map("Top", Map("X", 502, "Y", 1344, "Color", ""), "Bottom", Map("X", 477, "Y", 1432, "Color", "0xF9D799"), "Time", "", "Key", "2")
_flask3 := Map("Top", Map("X", 563, "Y", 1344, "Color", ""), "Bottom", Map("X", 539, "Y", 1432, "Color", "0xF9D799"), "Time", "", "Key", "3")
_flask4 := Map("Top", Map("X", 624, "Y", 1344, "Color", ""), "Bottom", Map("X", 600, "Y", 1432, "Color", "0xF9D799"), "Time", "", "Key", "4")
_flask5 := Map("Top", Map("X", 685, "Y", 1344, "Color", ""), "Bottom", Map("X", 661, "Y", 1432, "Color", "0xF9D799"), "Time", "", "Key", "5")

_tincture1 := Map("Top", Map("X", 463, "Y", 1345, "Color", "0xC47129"), "Bottom", _flask1["Bottom"], "Time", "", "Key", "1")
_tincture2 := Map("Top", Map("X", 524, "Y", 1344, "Color", "0xF2D33D"), "Bottom", _flask2["Bottom"], "Time", "", "Key", "2")
_tincture3 := Map("Top", Map("X", 584, "Y", 1342, "Color", "0xDA8857"), "Bottom", _flask3["Bottom"], "Time", "", "Key", "3")
_tincture4 := Map("Top", Map("X", 646, "Y", 1346, "Color", "0xAB461C"), "Bottom", _flask4["Bottom"], "Time", "", "Key", "4")
_tincture5 := Map("Top", Map("X", 707, "Y", 1343, "Color", "0xF3D344"), "Bottom", _flask5["Bottom"], "Time", "", "Key", "5")

_health := Map("X", 154, "Y", 1294, "Color", "0xA11822", "Time", "")
_mana := Map("X", 2409, "Y", 1399, "Color", "0x112348", "Time", "")

#HotIf WinActive(_windowTitle)
~$*F2::
{
    if(_flag)
    {
        return
    }

    global _flag := true
    
    _bloodrageTime := A_Now

    while(_flag && WinActive(_windowTitle))
    {
        if(DateDiff(_bloodrageTime, A_Now, "S") < 0)
        {
            SendInput(_abilityKeys["T"]["D"])
            Sleep(5)
            SendInput(_abilityKeys["T"]["U"])

            _bloodrageTime := DateAdd(A_Now, 8, "S")
        }

        local healthTime := _health["Time"]
        Health(_health["X"], _health["Y"], _health["Color"], &healthTime, _flaskKeys["1"])
        _health["Time"] := healthTime
        Sleep(5)
        local flask2Time := _flask2["Time"]
        FlaskRefresh(_flask2["Top"]["X"], _flask2["Top"]["Y"], _flask2["Top"]["Color"], _flask2["Bottom"]["X"], _flask2["Bottom"]["Y"], _flask2["Bottom"]["Color"], &flask2Time, _flask2["Key"])
        _flask2["Time"] := flask2Time
        Sleep(5)
        local flask3Time := _flask3["Time"]
        FlaskRefresh(_flask3["Top"]["X"], _flask3["Top"]["Y"], _flask3["Top"]["Color"], _flask3["Bottom"]["X"], _flask3["Bottom"]["Y"], _flask3["Bottom"]["Color"], &flask3Time, _flask3["Key"])
        _flask3["Time"] := flask3Time
        Sleep(5)
        local flask4Time := _flask4["Time"]
        FlaskRefresh(_flask4["Top"]["X"], _flask4["Top"]["Y"], _flask4["Top"]["Color"], _flask4["Bottom"]["X"], _flask4["Bottom"]["Y"], _flask4["Bottom"]["Color"], &flask4Time, _flask4["Key"])
        _flask4["Time"] := flask4Time
        Sleep(5)
        local flask5Time := _flask5["Time"]
        FlaskRefresh(_flask5["Top"]["X"], _flask5["Top"]["Y"], _flask5["Top"]["Color"], _flask5["Bottom"]["X"], _flask5["Bottom"]["Y"], _flask5["Bottom"]["Color"], &flask5Time, _flask5["Key"])
        _flask5["Time"] := flask5Time
    }

    global _flag := false
}
~$*F4::
{
    if(!_flag)
    {
        return
    }

    global _flag := false
}
~$*F5::
{
    RegisterUtilityFlaskColor()
}
#HotIf

Health(x, y, color, &time, key)
{
    if(DateDiff(time, A_Now, "S") < 0)
    {
        if(PixelGetColor(x, y) != color)
        {
            SendInput(key["D"])
            Sleep(5)
            SendInput(key["U"])
        }

        time := DateAdd(A_Now, 1, "S")
    }
}
Mana(x, y, color, key)
{
    if(PixelGetColor(x, y) != color)
    {
        SendInput(key["D"])
        Sleep(5)
        SendInput(key["U"])
    }
}
FlaskRefresh(x1, y1, color1, x2, y2, color2, &time, key)
{
    if(DateDiff(time, A_Now, "S") < 0)
    {
        if(PixelGetColor(x1, y1) = color1 && PixelGetColor(x2, y2) != color2)
        {
            SendInput(key["D"])
            Sleep(5)
            SendInput(key["U"])
        }

        time := DateAdd(A_Now, 2, "S")
    }
}
RegisterUtilityFlaskColor()
{
    _flask1["Top"]["Color"] := PixelGetColor(_flask1["Top"]["X"], _flask1["Top"]["Y"])
    _flask2["Top"]["Color"] := PixelGetColor(_flask2["Top"]["X"], _flask2["Top"]["Y"])
    _flask3["Top"]["Color"] := PixelGetColor(_flask3["Top"]["X"], _flask3["Top"]["Y"])
    _flask4["Top"]["Color"] := PixelGetColor(_flask4["Top"]["X"], _flask4["Top"]["Y"])
    _flask5["Top"]["Color"] := PixelGetColor(_flask5["Top"]["X"], _flask5["Top"]["Y"])

    _flask1["Time"] := A_Now
    _flask2["Time"] := A_Now
    _flask3["Time"] := A_Now
    _flask4["Time"] := A_Now
    _flask5["Time"] := A_Now

    _health["Time"] := A_Now
}

_flaskKeys["1"] := GetMap(_flaskKeys["1"])
_flaskKeys["2"] := GetMap(_flaskKeys["2"])
_flaskKeys["3"] := GetMap(_flaskKeys["3"])
_flaskKeys["4"] := GetMap(_flaskKeys["4"])
_flaskKeys["5"] := GetMap(_flaskKeys["5"])

_abilityKeys["Q"] := GetMap(_abilityKeys["Q"])
_abilityKeys["W"] := GetMap(_abilityKeys["W"])
_abilityKeys["E"] := GetMap(_abilityKeys["E"])
_abilityKeys["R"] := GetMap(_abilityKeys["R"])
_abilityKeys["T"] := GetMap(_abilityKeys["T"])
_abilityKeys["A"] := GetMap(_abilityKeys["A"])
_abilityKeys["S"] := GetMap(_abilityKeys["S"])
_abilityKeys["D"] := GetMap(_abilityKeys["D"])
_abilityKeys["F"] := GetMap(_abilityKeys["F"])
_abilityKeys["G"] := GetMap(_abilityKeys["G"])

_flask1["Key"] := _flaskKeys["1"]
_flask2["Key"] := _flaskKeys["2"]
_flask3["Key"] := _flaskKeys["3"]
_flask4["Key"] := _flaskKeys["4"]
_flask5["Key"] := _flaskKeys["5"]

_tincture1["Key"] := _flaskKeys["1"]
_tincture2["Key"] := _flaskKeys["2"]
_tincture3["Key"] := _flaskKeys["3"]
_tincture4["Key"] := _flaskKeys["4"]
_tincture5["Key"] := _flaskKeys["5"]

GetMap(key)
{
    _map := Map()

    key := GetKeyVK(key)
    
    _map["K"] := Format("{Blind}{VK{:X}}", key)
    _map["D"] := Format("{Blind}{VK{:X} Down}", key)
    _map["U"] := Format("{Blind}{VK{:X} Up}", key)
    
    return _map
}