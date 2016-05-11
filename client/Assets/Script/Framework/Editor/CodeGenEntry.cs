﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Framework
{
    public class CodeGenEntry : MonoBehaviour
    {
        [MenuItem("CellOrigin/生成选中对象框架代码")]
        public static void GenCodeBySelectedObject()
        {
            var rootObj = Selection.activeGameObject;

            //GenCodeByPrefab(rootObj, null );
        }

        static void GenCodeByPrefab( GameObject rootObj, gamedef.CodeGenModule module )
        {
            if (rootObj == null)
                return;

            if ( rootObj.name != module.Name )
            {
                Debug.LogError("请保证 Prefab文件名, Prefab名, Module名 三者一致: " + module.Name);
                return;
            }

            var rootContext = rootObj.GetComponent<DataContext>();
            if (rootContext == null)
            {
                Debug.LogError("根对象组件类型没有发现DataContext组件");
                return;
            }


            if (rootContext.Type != WidgetType.View && rootContext.Type != WidgetType.ItemView)
            {
                Debug.LogError("根对象组件类型必须是View类型");
                return;
            }

            var ctxList = new List<DataContext>();

            foreach (Transform childTrans in rootObj.transform)
            {
                IterateDataContext(childTrans, ref ctxList);
            }

            {
                var gen = new CodeGenerator();

                ViewTemplate.Delete(rootContext);
                ViewTemplate.ClassBody(gen, rootContext, ctxList);
                ViewTemplate.Save(gen, rootContext);
            }

            if (!module.NoGenPresenterCode)
            {
                var gen = new CodeGenerator();

                PresenterTemplate.Delete(rootContext);
                PresenterTemplate.ClassBody(gen, rootContext, ctxList, module);
                PresenterTemplate.Save(gen, rootContext);
            }



            AssetDatabase.Refresh();
        }

        [MenuItem("CellOrigin/从文件生成框架代码")]
        public static void GenCodeByFile()
        {
            var file = ProtobufUtility.LoadTextFile<gamedef.CodeGenFile>("Assets/Script/CodeGen/ui.pbt");

            GenCodeByManifest(file);
        }

        static void GenCodeByManifest( gamedef.CodeGenFile file )
        {
            foreach( gamedef.CodeGenModule module in file.CodeGen )
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>( string.Format("Assets/Resources/View/{0}.prefab", module.Name) );

                GenCodeByPrefab(prefab, module );
            }
        }
        

        static bool CheckDuplicateName( List<DataContext> list, string name )
        {
            foreach( DataContext ctx in list )
            {
                if (ctx.Name == name)
                    return true;
            }

            return false;
        }


        static void IterateDataContext(Transform trans, ref List<DataContext> ctxList )
        {
            var childContext = trans.GetComponent<DataContext>();
            if (childContext != null)
            {
                if ( CheckDuplicateName(  ctxList, childContext.Name ) )
                {
                    throw new Exception("重名" + childContext.Name);
                }

                ctxList.Add(childContext);
            }

            foreach (Transform childTrans in trans)
            {
                IterateDataContext(childTrans, ref ctxList);
            }
        }
    }

}