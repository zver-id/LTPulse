import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import apiUrl from "./api-url.ts";

export interface IMetricGroup {
    id: number;
    name: string;
    nameOfChart: string;
    chartType: string
}

export const metricGroupsApi = createApi({
    reducerPath: "metricGroups",
    baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/MetricGroup`}),
    tagTypes: ['metricGroups'],
    endpoints: (builder) => ({
        getMetricGroups: builder.query<IMetricGroup[], void>({
            query: () => '',
            providesTags: ['metricGroups']
        })
    })
});

export const { useGetMetricGroupsQuery } = metricGroupsApi;