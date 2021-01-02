using System.Runtime.CompilerServices;

namespace Unity.Entities
{

    static public class EntityNameExtension
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SetName_( this EntityManager em, Entity ent, string name )
        {
#if UNITY_EDITOR
            em.SetName(ent, name);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string GetName_(this EntityManager em, Entity ent)
        {
#if UNITY_EDITOR
            return em.GetName(ent);
#else
            return "";
#endif
        }

    }

}