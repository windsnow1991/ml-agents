using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.MLAgents.Sensors.Reflection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ObservableAttribute : Attribute
    {
        string m_Name;

        const BindingFlags k_BindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static Dictionary<Type, int> s_TypeSizes = new Dictionary<Type, int>()
        {
            {typeof(int), 1},
            {typeof(bool), 1},
            {typeof(float), 1},
            {typeof(Vector2), 2},
            {typeof(Vector3), 3},
            {typeof(Vector4), 4},
            {typeof(Quaternion), 4},
        };

        public ObservableAttribute(string name=null)
        {
            m_Name = name;
        }

        internal static List<ISensor> GetObservableSensors(object o)
        {
            var sensorsOut = new List<ISensor>();

            var fields = o.GetType().GetFields(k_BindingFlags);
            foreach (var field in fields)
            {
                var attr = (ObservableAttribute)GetCustomAttribute(field, typeof(ObservableAttribute));
                if (attr != null)
                {
                    sensorsOut.Add(CreateReflectionSensor(o, field, null, attr));
                }
            }

            var properties = o.GetType().GetProperties(k_BindingFlags);
            foreach (var prop in properties)
            {
                var attr = (ObservableAttribute)GetCustomAttribute(prop, typeof(ObservableAttribute));
                if (attr != null)
                {
                    sensorsOut.Add(CreateReflectionSensor(o, null, prop, attr));
                }
            }
            return sensorsOut;
        }

        internal static ISensor CreateReflectionSensor(object o, FieldInfo fieldInfo, PropertyInfo propertyInfo, ObservableAttribute observableAttribute)
        {
            string memberName;
            string declaringTypeName;
            Type memberType;
            if (fieldInfo != null)
            {
                declaringTypeName = fieldInfo.DeclaringType.Name;
                memberName = fieldInfo.Name;
                memberType = fieldInfo.FieldType;
            }
            else
            {
                declaringTypeName = propertyInfo.DeclaringType.Name;
                memberName = propertyInfo.Name;
                memberType = propertyInfo.PropertyType;
            }

            string sensorName;
            if (string.IsNullOrEmpty(observableAttribute.m_Name))
            {
                sensorName = $"ObservableAttribute:{declaringTypeName}.{memberName}";
            }
            else
            {
                sensorName = observableAttribute.m_Name;
            }

            var reflectionSensorInfo = new ReflectionSensorInfo
            {
                Object = o,
                FieldInfo = fieldInfo,
                PropertyInfo = propertyInfo,
                ObservableAttribute = observableAttribute,
                SensorName = sensorName
            };

            if (memberType == typeof(Int32))
            {
                return new IntReflectionSensor(reflectionSensorInfo);
            }
            if (memberType == typeof(float))
            {
                return new FloatReflectionSensor(reflectionSensorInfo);
            }
            if (memberType == typeof(bool))
            {
                return new BoolReflectionSensor(reflectionSensorInfo);
            }
            if (memberType == typeof(Vector2))
            {
                return new Vector2ReflectionSensor(reflectionSensorInfo);
            }
            if (memberType == typeof(Vector3))
            {
                return new Vector3ReflectionSensor(reflectionSensorInfo);
            }
            if (memberType == typeof(Vector4))
            {
                return new Vector4ReflectionSensor(reflectionSensorInfo);
            }
            if (memberType == typeof(Quaternion))
            {
                return new QuaternionReflectionSensor(reflectionSensorInfo);
            }

            throw new UnityAgentsException($"Unsupported Observable type: {memberType.Name}");

        }

        internal static int GetTotalObservationSize(object o)
        {
            int sizeOut = 0;

            var fields = o.GetType().GetFields(k_BindingFlags);
            foreach (var field in fields)
            {
                var attr = (ObservableAttribute)GetCustomAttribute(field, typeof(ObservableAttribute));
                if (attr != null)
                {
                    sizeOut += s_TypeSizes[field.FieldType];
                }
            }

            var properties = o.GetType().GetProperties(k_BindingFlags);
            foreach (var prop in properties)
            {
                var attr = (ObservableAttribute)GetCustomAttribute(prop, typeof(ObservableAttribute));
                if (attr != null)
                {
                    sizeOut += s_TypeSizes[prop.PropertyType];
                }
            }
            return sizeOut;
        }

    }

}
