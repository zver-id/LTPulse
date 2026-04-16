import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import apiUrl from "./api-url.ts";

export interface IMetricGroup {
    key: number;
    name: string;
    nameOfChart: string;
    typeOfChart: string
}

export const metricGroupsApi = createApi({
    reducerPath: "metricGroups",
    baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/MetricGroups`}),
    tagTypes: ['metricGroups'],
    endpoints: (builder) => ({
        getMetricGroups: builder.query<IMetricGroup[], void>({
            query: () => '',
            providesTags: ['metricGroups']
        })
    })
});

export const { useGetMetricGroupsQuery } = metricGroupsApi;