using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DotsLite.Structure.Authoring
{
    using DotsLite.Geometry;

    public interface IStructurePart
    {
        int partId { get; set; }
        IEnumerable<IMeshModel> QueryModel { get; }
    }
}
/*
エリア
・子構造物

・地形フィット                                     ボタン     o
・メッシュフィット　テセレート                      　ボタン
・パス変形は描画メッシュ用　デブリはシェーダで変形       
　- デブリはプレハブフェーズで事前生成


・パス上面　補間／垂直                             パーツ単位
・パスメッシュ　無変換                             パーツ単位   o



構造物　エリア
・パーツ単位のエンティティ／ボーン単位のエンティティ

・デブリ　あり／なし                              パーツ単位
・デブリはプレハブキー依存

・ボーン　フレーム／固定バッファ                    構造物単位

・パレット
・隣接パーツ
*/