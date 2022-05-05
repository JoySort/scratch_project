using System;
using System.Collections.Generic;
using System.Linq;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;

namespace UnitTest.Worker;

public class Utilizer
{
    /**
     * <summary>Return list first element is Dictionary<long, long> triggerid->selectedOutletIndex, second element is the List<RecResult> object list</summary>
     */
    public static List<Object> prepareData(Project project,long startTriggerID, long count,int columnCount,int perTargetPictureCount,int columCountPerSection )
    {

        
        
        var result = new List<RecResult>();
        var critieraList = project.Criterias;
        var outlets = ConfigUtil.getModuleConfig().SortConfig.OutletPriority==OutletPriority.DESC
            ?project.Outlets.OrderByDescending(outlet => outlet.ChannelNo).ToArray()
            :project.Outlets.OrderBy(outlet => outlet.ChannelNo).ToArray();
        var consolidatePolicy = ConfigUtil.getModuleConfig().ConsolidatePolicy;
        Random rdn = new Random();
        var selectedOutletIndex = 0;
        var currentOutletIndex = 0;
        var triggerIDWithSelectedChannelNO = new Dictionary<long, int>();
        for(var triggerIdIndex=startTriggerID; triggerIdIndex< (startTriggerID+count) ; triggerIdIndex++){
            
            //针对每个triggerid,找到一个filter不为空的outlet,以这个outlet的filter来生成一个符合这个filter的RecResult
            while(true)
            {
                selectedOutletIndex = currentOutletIndex % outlets.Length;
               // logger.Debug("selectedOutletIndex,currentOutletIndex {}.{}",selectedOutletIndex,currentOutletIndex);
                if (outlets[selectedOutletIndex].Filters.Length > 0)
                {
                    
                    currentOutletIndex++;
                    break;
                }
                else
                {
                    currentOutletIndex++;
                }

            }
           // logger.Debug("outlet index {}",selectedOutletIndex);
           triggerIDWithSelectedChannelNO.Add(triggerIdIndex,selectedOutletIndex);
            for(var col=0; col<columnCount;col++)
            {
                var expectedFeatureCount = critieraList.Length;
                int NotNormalOffsetRowCount = -1;
                int NotNormalOffsetRowCountIndex = -1;
                Filter offsetRowNotNormalFeature = null;
                Dictionary<int,Filter> features= new Dictionary<int,Filter> ();
                for (var featureIndex = 0; featureIndex < expectedFeatureCount; featureIndex++)
                {
                        
                        var found = false;
                        var rowOffsetNormal = false;
                        
                        for (var i = 0; i < consolidatePolicy.CriteriaCode.Length; i++)
                        {
                            if (consolidatePolicy.CriteriaCode[i] ==critieraList[featureIndex].Code)
                            {
                                if (consolidatePolicy.OffSetRowCount[i] == perTargetPictureCount)
                                {
                                    rowOffsetNormal = true;
                                }
                                else
                                {
                                    NotNormalOffsetRowCount = consolidatePolicy.OffSetRowCount[i];
                                    NotNormalOffsetRowCountIndex = ConfigUtil.getModuleConfig().CriteriaMapping[consolidatePolicy.CriteriaCode[i]].Index;
                                }
                            }

                        }
                        
                        foreach (var OrFilters in outlets[selectedOutletIndex].Filters)
                        {
                            foreach (var andFilter in OrFilters)
                            {
                                if (andFilter.Criteria.Code == critieraList[featureIndex].Code)
                                {
                                    //判断是否是照片里面的feature(rowOffset跟照片数量偏差是否一样）如果是，正常添加feature,如果不是，则需要添加一个相差了rowoffset数量的RecRecord
                                    if(rowOffsetNormal){
                                        if(!features.ContainsKey(critieraList[featureIndex].Index))features.Add(critieraList[featureIndex].Index,andFilter);
                                    }
                                    else
                                    {
                                        offsetRowNotNormalFeature = andFilter;
                                    }

                                    found = true;
                                }
                               


                                
                            }
                            
                        }
                        
                        
                       
                        //通道没有选择这个分选标准（其他通道可能选择了），并且不是类似称重这种单独发送一个feature过来的情况，增加一个不在范围内的数字即可。
                        if (!found && rowOffsetNormal)
                        {
                            if(rowOffsetNormal){
                                features.Add(critieraList[featureIndex].Index,
                                    null); 
                            }
                        }
                        
                        //通道没有这个分选标准（其他通道可能选择了），但是，feature是一个类似称重的offset跟拍照数量不一样的情况，也就是拍照结果无法得到这个feature。就模拟不添加。
                        if (!found && !rowOffsetNormal)
                        {
                            //do nothing
                        }
                        
                        


                        //if(coordinate.TriggerId<8 && coordinate.Section==0 && coordinate.Column == 0)logger.Debug("feature {},{},{},{}",selectedOutletIndex,coordinate.Key(),features.Last().Value.CriteriaIndex,features.Last().Value.Value);

                }
                    
                float section = col / columCountPerSection;
                Coordinate coordinate = null;
                for (var row = 0; row < perTargetPictureCount; row++)
                {
                    
                    coordinate = new Coordinate(col, row, CameraPosition.middle,triggerIdIndex);
                    var featureList = new List<Feature>();
                    foreach ((var key,var value) in features)
                    {
                        float featureValue = -1;
                        //目前filter为了支持以后更富在的filter,允许在andFilter提供2个范围，实际上，如果是两个范围，是无法形成and的，所以，我们
                        //目前虽然按照数组给值，仍然是给一个值，所以下面用First()拿到的就是这个Filter的界限。
                        if(value!=null){
                            var diff = value.FilterBoundaries.First().Last() -
                                       value.FilterBoundaries.First().First();
                            //生成一个不小于下边界，不大于上边界的随机数。
                             featureValue = value.FilterBoundaries.First().First() + ((float)rdn.Next((int)diff*100))/100;
                        }
                        featureList.Add(new Feature(key,featureValue));
                    }
                    
                    var recResult = new RecResult(coordinate, expectedFeatureCount, DateTimeOffset.Now.ToUnixTimeMilliseconds(),featureList);
                    result.Add(recResult);
                }
                
                
                
                 coordinate = new Coordinate( col, NotNormalOffsetRowCount-1,CameraPosition.middle, triggerIdIndex);
                 var featureList1 = new List<Feature>();
                if (offsetRowNotNormalFeature != null)
                {
                    var diff = offsetRowNotNormalFeature.FilterBoundaries.First().Last() -
                               offsetRowNotNormalFeature.FilterBoundaries.First().First();
                    //生成一个不小于下边界，不大于上边界的随机数。
                    var featureValue = offsetRowNotNormalFeature.FilterBoundaries.First().First() + ((float)rdn.Next((int)diff*100))/100;
                    featureList1.Add(new Feature(offsetRowNotNormalFeature.Criteria.Index,featureValue));
                    var recResult1 = new RecResult(coordinate, expectedFeatureCount,DateTimeOffset.Now.ToUnixTimeMilliseconds(), featureList1);
                    result.Add(recResult1);
                }
                else if(NotNormalOffsetRowCountIndex!=-1)
                {
                    featureList1.Add(new Feature(NotNormalOffsetRowCountIndex,0));
                    var recResult1 = new RecResult(coordinate, expectedFeatureCount,DateTimeOffset.Now.ToUnixTimeMilliseconds(), featureList1);
                    result.Add(recResult1);
                }
                


            }
        }

        var returnObj = new List<Object>();
        returnObj.Add(triggerIDWithSelectedChannelNO);
        returnObj.Add(result);
        return returnObj;
    }
}