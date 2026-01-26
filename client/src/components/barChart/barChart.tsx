import { useGetFilteredMetricsQuery } from "../../storage/services/metrics-api.ts";

function metricBarChart({data}){
    const { data, isLoading, isError } = useGetFilteredMetricsQuery();
    return  <h1>fgh</h1>
}

export default metricBarChart;