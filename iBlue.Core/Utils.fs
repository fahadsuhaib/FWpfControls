namespace iBlue.Core
open System
open System.Collections
open System.Linq
open System.ComponentModel

module PropertyDescriptorExtensions =
    type PropertyDescriptorCollection with
        member this.GetValue(columnName : String, record : obj) =
            let propertyNameList = columnName.Split('.')
            let complexPropertyCount = propertyNameList.Count()
            let mutable tPdc = this
            let mutable pd = null
            let mutable tRecord = record
            for iterator = 0 to complexPropertyCount - 1 do
                pd <- tPdc.Find(propertyNameList.[iterator], true)
                if iterator <> complexPropertyCount - 1 then
                    tRecord <- pd.GetValue(tRecord)
                tPdc <- TypeDescriptor.GetProperties(pd.PropertyType)
            if pd <> null then
                pd.GetValue(tRecord)
            else
                null
