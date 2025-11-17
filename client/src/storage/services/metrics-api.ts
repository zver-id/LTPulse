import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type { MonthData } from "../../types/month-data.ts";

export const metricsApi = createApi({
    reducerPath: "metrics",
    baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api`}),
    endpoints: (builder) => ({
        getAllMetrics: builder.query<MonthData[], { teamId: number, dayCount: number}>({
            query: ({teamId, dayCount})=> `Metrics?teamId=${teamId}&dayCount=${dayCount}`,
        })
    })
});

export const { useGetAllMetricsQuery } = metricsApi;