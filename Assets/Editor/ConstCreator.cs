//  EnumCreator.cs
//  http://kan-kikuchi.hatenablog.com/entry/EnumCreator
//
//  Created by kan.kikuchi on 2016.08.30.

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Enumを生成するクラス
/// </summary>
public static class ConstCreator {

  //コード全文とタブ文字
  private static string _code = "", _tab = "";

  //=================================================================================
  //共通
  //=================================================================================

  //初期化し、enumの上部部分を作成
  private static void Init(string nameSpace, string summary, bool isFlags, string enumName){
    //コード全文とタブ文字をリセット
    _code = "";
    _tab  = "";

    //ネームスペースが入力されていれば設定
    if(!string.IsNullOrEmpty(nameSpace)){
      _code += "namespace " + nameSpace + "{\n";
      _tab  += "\t";
    }

    //概要が入力されていれば設定
    if (!string.IsNullOrEmpty (summary)) {
      _code += 
        _tab + "/// <summary>\n" +
        _tab + "/// " + summary + "\n" +
        _tab + "/// </summary>\n";
    }

    //フラグ属性の設定
    if (isFlags) {
      _code += _tab + "[System.Flags]\n";
    }

    //enum名を設定
    _code += _tab + "static public class " + enumName + "\n{\n";

    //インデントを下げる
    _tab += "\t\t";
  }

  //enumを書き出し
  private static void Export(string exportPath, string nameSpace, string enumName){
    //ネームスペースが入力されていれば設定
    if(!string.IsNullOrEmpty(nameSpace)){
      _code += "\t}\n";
    }

    _code += "}";

    //ファイルの書き出し
    File.WriteAllText (exportPath, _code, Encoding.UTF8);
    AssetDatabase.Refresh (ImportAssetOptions.ImportRecursive);

    Debug.Log (enumName + "の作成が完了しました");
  }

  //=================================================================================
  //Enum生成
  //=================================================================================

  /// <summary>
  /// Enumを生成する
  /// </summary>
  //public static void Create(string enumName, List<string>itemNameList, string exportPath,  string summary = "", string nameSpace = "", bool isFlags = false){
  public static void Create
    (string enumName, IEnumerable<string>itemNameList, string exportPath,  string summary = "", string nameSpace = "", bool isFlags = false)
  {
    //初期化
    Init(nameSpace, summary, isFlags, enumName);

    //定数名の最大長を取得し、空白数を決定(イコールが並ぶように)
    int nameLengthMax = 0;
    if(itemNameList.Any()){
      nameLengthMax = itemNameList.Select (name => name.Length).Max ();
    }

    //各項目を設定
    foreach( var (i, itemName) in itemNameList.Select( (x,i) => (i,x) ) )
    {
      _code += $"{_tab}public const int {itemName} = ";

      if(isFlags){
        _code += " 1 << " + i.ToString() + ";\n";
      }
      else{
        _code += " " + i.ToString() + ";\n";
      }
    }

    //書き出し
    Export(exportPath, nameSpace, enumName);
  }

  /// <summary>
  /// Enumを生成する
  /// </summary>
  public static void Create(string enumName, Dictionary<string, int>itemDict, string exportPath, string summary = "", string nameSpace = ""){
    //初期化
    Init(nameSpace, summary, false, enumName);

    //定数名の最大長を取得し、空白数を決定
    int nameLengthMax = 0;
    if(itemDict.Keys.Count > 0){
      nameLengthMax = itemDict.Keys.Select (name => name.Length).Max ();
    }

    //各項目を設定
    foreach (KeyValuePair<string, int> item in itemDict) {
      _code += _tab + item.Key;
      _code += " " + String.Format("{0, " + (nameLengthMax - item.Key.Length + 1).ToString() + "}", "=");
      _code += " " + item.Value.ToString() + ",\n";
    }

    //書き出し
    Export(exportPath, nameSpace, enumName);
  }


}