import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type { TeamData } from "../../types/team-data.ts";

export const metricsApi = createApi({
    reducerPath: "metrics",
    baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api`}),
    endpoints: (builder) => ({
        getAllMetrics: builder.query<TeamData[], { teamId: number, dayCount: number}>({
            query: ({teamId, dayCount})=>
                `Metrics?teamId=${teamId}&dayCount=${dayCount}`,
        }),

        getFilteredMetrics: builder.query<TeamData[], { teamId: number, dayCount: number, filter: string }>({
            query: ({teamId, dayCount, filter}) =>
                `Metrics/filtered?teamId=${teamId}&dayCount=${dayCount}&filterSing=${filter}`,
        })
    })
});

export const { useGetAllMetricsQuery,
               useGetFilteredMetricsQuery } = metricsApi;