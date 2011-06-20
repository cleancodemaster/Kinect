﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using System.Windows;
using log4net;

namespace Common
{
    public static class ClassExtensions
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ClassExtensions));

        public static void Raise(this PropertyChangedEventHandler handler, Expression<Func<object>> propertyExpression)
        {
            if (handler != null)
            {
                // Retreive lambda body
                var body = propertyExpression.Body as MemberExpression;
                if (body == null)
                {
                    _log.WarnFormat("Not a member expression: ", propertyExpression.Body);
                    return;
                }

                // Extract the right part (after "=>")
                var vmExpression = body.Expression as ConstantExpression;
                if (vmExpression == null)
                {
                    throw new ArgumentException("'propertyExpression' body should be a constant expression");
                }

                // Create a reference to the calling object to pass it as the sender
                LambdaExpression vmlambda = System.Linq.Expressions.Expression.Lambda(vmExpression);
                Delegate vmFunc = vmlambda.Compile();
                object vm = vmFunc.DynamicInvoke();

                // Extract the name of the property to raise a change on
                string propertyName = body.Member.Name;
                var e = new PropertyChangedEventArgs(propertyName);
                handler(vm, e);
            }
        }

        public static ObservableCollection<T> CreateCopy<T>(this ObservableCollection<T> collection) where T : ICopyAble<T>
        {

            var returnValue = new ObservableCollection<T>();
            foreach (var element in collection)
            {
                if (element != null)
                {
                    returnValue.Add(element.CreateCopy());
                }
                else
                {
                    returnValue.Add(element);
                }
            }
            return returnValue;
        }

        public static Point3D ToScreenPosition(this Point3D point, Size cameraResolution, Size screenResolution) 
        {
            return point.ToScreenPosition(cameraResolution,screenResolution,new Point(0,0), cameraResolution);
        }

        public static Point3D ToScreenPosition(this Point3D point, Size cameraResolution, Size screenResolution, Point startPoint, Size HandResolution)
        {
            return point.ToScreenPosition(cameraResolution,screenResolution,startPoint,HandResolution,false);
        }

        public static Point3D ToScreenPosition(this Point3D point, Size cameraResolution, Size screenResolution, Point startPoint, Size HandResolution, bool keepInBounds)
        {
            var customX = point.X - startPoint.X;
            var customY = point.Y - startPoint.Y;

            if(keepInBounds)
            {
                customX = customX < 0 ? 0 : customX;
                customX = customX > HandResolution.Width ? HandResolution.Width : customX;

                customY = customY < 0 ? 0 : customY;
                customY = customY > HandResolution.Height ? HandResolution.Height : customY;
            }

            var x = screenResolution.Width / HandResolution.Width * customX;
            var y = screenResolution.Height / HandResolution.Height * customY;
            var z = 75 - point.Z / 30;
            
            if (z < 10)
            {
                z = 10;
            }

            return new Point3D(x, y, z);
        }

        public static string GetDebugString(this Point3D point)
        {
            return string.Format("X:{0:0}, Y:{1:0}, Z:{2:0}", point.X, point.Y, point.Z);
        }
    }
}