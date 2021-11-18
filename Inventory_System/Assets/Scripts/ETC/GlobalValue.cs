using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValue
{
    [HideInInspector] public static string g_Unique_ID = "";
    [HideInInspector] public static string g_Unique_PW = "";
    [HideInInspector] public static string g_NickName = "";   //유저의 별명
    [HideInInspector] public static int g_Level = 1;                           // 유저 레벨
    [HideInInspector] public static string g_UserName = "";                    // 유저 이름
    [HideInInspector] public static CharType g_CharType = CharType.Swordman;   // 유저 직업
    [HideInInspector] public static int g_Sp = 0;
}
