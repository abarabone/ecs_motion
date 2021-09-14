using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sprache;

namespace MqoUtility
{
    static public class MqoParser
    {

        static public (string name, Vector3[] vtx, int[][] tris)[] ConvertToObjectsData( string s )
        {

            var qNotQuote =
                from text in Parse.AnyChar.Except( Parse.Char( '"' ) ).Many().Text()
                select text
                ;
            Func<string, Parser<string>> qSkipUntil = target =>
                from _ in Parse.AnyChar.Except( Parse.String( target ) ).Many().Text()
                select _
                ;

            Func<string, Parser<string>> qTagName = tag =>
                from _ in qSkipUntil( tag )
                from obj in Parse.String( tag )
                from tag_name in qNotQuote.Contained( Parse.Char( '"' ), Parse.Char( '"' ) ).Token()
                select tag_name
                ;
            Func<string, Parser<string>> qTagContent = tag =>
                from _ in qSkipUntil( tag )
                from name in Parse.String( tag )
                from __ in qSkipUntil( "{" )
                from content in Parse.CharExcept( '}' ).Many().Contained( Parse.Char( '{' ), Parse.Char( '}' ) ).Text()
                select content
                ;
            var qObjectContent =
                from _st in qSkipUntil( "{" )
                from vtx in qTagContent( "vertex" )
                from face in qTagContent( "face" )
                from _ed in qSkipUntil( "}" )
                select (vtx, face)
                ;
            var qObject =
                from _ in qSkipUntil( "Object" )
                from obj_name in qTagName( "Object" )
                from content in qObjectContent
                select (obj_name, content.vtx, content.face)
                ;
            var qAllObjects =
                from objects in qObject.Many()
                select objects
                ;
            var objectTexts = qAllObjects.Parse( s );


            var qExponent =
                from _ in Parse.Char( 'E' )
                from sign in Parse.Chars( "+-" )
                from num in Parse.Number
                select $"E{sign}{num}"//String.Format( "E{0}{1}", sign, num )
                ;
            var qFloatEx =
                from negative in Parse.Char( '-' ).Optional().Select( x => x.IsDefined ? x.Get().ToString() : "" )
                from num in Parse.Decimal
                from e in qExponent.Optional().Select( x => x.IsDefined ? x.Get() : "" )
                select Convert.ToSingle( negative + num + e )
                ;
            var qVtx =
                from x in qFloatEx.Token()
                from y in qFloatEx.Token()
                from z in qFloatEx.Token()
                select new Vector3( -x, y, z )// X軸反転
                ;
            var qIdx =
                from corner_length in Parse.Decimal.Token().Select( x => int.Parse( x ) )
                from index_body in Parse.Numeric.Or( Parse.WhiteSpace ).Many().Contained( Parse.String( "V(" ), Parse.Char( ')' ) ).Token().Text()
                from _ in Parse.AnyChar.Until( Parse.LineEnd )
                select (corner_length, indices: index_body.Split( ' ' ).Select( x => int.Parse( x ) ).ToArray())// インデックスは逆まわりなので、反転は不要
                ;

            var data = objectTexts
                .Select( x => toObjectsData_( x.obj_name, x.vtx, x.face ) )
                .ToArray();

            return data;


            (string name, Vector3[] vtxs, int[][] tris) toObjectsData_( string name, string vtx, string face )
            {
                //Debug.Log( $"{txt.obj_name} {txt.vtx} {txt.face}" );
                var vtxs = qVtx.Many().Parse( vtx );
                var tris = qIdx.Where( x => x.corner_length == 3 ).Select( x => x.indices ).Many().Parse( face );
                //foreach( var v in vtxs ) Debug.Log( $"{v[ 0 ]} {v[ 1 ]} {v[ 2 ]}" );
                //foreach( var t in tris ) Debug.Log( $"{t[ 0 ]} {t[ 1 ]} {t[ 2 ]}" );

                return (name, vtxs.ToArray(), tris.ToArray());
            }
        }
    }
}
