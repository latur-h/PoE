#Requires AutoHotkey v2.0
#SingleInstance

_windowTitle := "ahk_exe PathOfExile.exe"
_flag := false

_D1Key := "1"
_D2Key := "2"
_D3Key := "3"
_D4Key := "4"
_D5Key := "5"

_ability1Key := "Q"
_ability2Key := "W"
_ability3Key := "E"
_ability4Key := "R"
_ability5Key := "T"

_health := Map("X", 154, "Y", 1294, "Color", "A11822")
_mana := Map("X", 2409, "Y", 1399, "Color", "112348")

_flask1 := Map("X", 416, "Y", 1432, "Color", "F9D799")
_flask2 := Map("X", 478, "Y", 1432, "Color", "F9D799")
_flask3 := Map("X", 542, "Y", 1432, "Color", "F9D799")
_flask4 := Map("X", 602, "Y", 1432, "Color", "F9D799")
_flask5 := Map("X", 661, "Y", 1432, "Color", "F9D799")

_tincture1 := Map("X", 463, "Y", 1345, "Color", "C47129")
_tincture2 := Map("X", 524, "Y", 1344, "Color", "F2D33D")
_tincture3 := Map("X", 584, "Y", 1342, "Color", "DA8857")
_tincture4 := Map("X", 646, "Y", 1346, "Color", "AB461C")
_tincture5 := Map("X", 707, "Y", 1343, "Color", "F3D344")

#HotIf WinActive(_windowTitle)
~$*F2::
{
    if(_flag)
    {
        return
    }

    global _flag := true
    
    while(_flag && WinActive(_windowTitle))
    {
        SendInput(_ability5Key["D"])
        Sleep(5)
        SendInput(_ability5Key["U"])
        Sleep(100)
        Health(_health["X"], _health["Y"], _health["Color"], _D1Key)
        Sleep(100)
        Mana(_mana["X"], _mana["Y"], _mana["Color"], _D5Key)
        Sleep(100)
        FlaskRefresh(_flask2["X"], _flask2["Y"], _flask2["Color"], _D2Key)
        Sleep(200)
        FlaskRefresh(_flask3["X"], _flask3["Y"], _flask3["Color"], _D3Key)
        Sleep(200)
        FlaskRefresh(_flask4["X"], _flask4["Y"], _flask4["Color"], _D4Key)
        Sleep(200)
    }
}
~$*F4::
{
    if(!_flag)
    {
        return
    }
    global _flag := false
}
#HotIf

Health(x, y, color, key)
{
    if(PixelGetColor(x, y) != "0x" color)
    {
        SendInput(key["D"])
        Sleep(5)
        SendInput(key["U"])
    }
}
Mana(x, y, color, key)
{
    if(PixelGetColor(x, y) != "0x" color)
    {
        SendInput(key["D"])
        Sleep(5)
        SendInput(key["U"])
    }
}
FlaskRefresh(x, y, color, key)
{
    if(PixelGetColor(x, y) != "0x" color)
    {
        SendInput(key["D"])
        Sleep(5)
        SendInput(key["U"])
    }
}

_D1Key := GetMap(_D1Key)
_D2Key := GetMap(_D2Key)
_D3Key := GetMap(_D3Key)
_D4Key := GetMap(_D4Key)
_D5Key := GetMap(_D5Key)

_ability1Key := GetMap(_ability1Key)
_ability2Key := GetMap(_ability2Key)
_ability3Key := GetMap(_ability3Key)
_ability4Key := GetMap(_ability4Key)
_ability5Key := GetMap(_ability5Key)

GetMap(key)
{
    _map := Map()

    key := GetKeyVK(key)
    
    _map["K"] := Format("{Blind}{VK{:X}}", key)
    _map["D"] := Format("{Blind}{VK{:X} Down}", key)
    _map["U"] := Format("{Blind}{VK{:X} Up}", key)
    
    return _map
}