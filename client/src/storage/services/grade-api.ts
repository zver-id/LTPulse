import {createApi, fetchBaseQuery} from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type {IGrade} from "../../components/gradeScreen/gradeScreen.tsx";

export interface GradeResponse {
  schema: Record<string, string>;
  data: IGrade[]
}

export const gradeApi = createApi({
  reducerPath: "grade",
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/Grade`}),
  tagTypes: ['grades'],
  endpoints: (builder) => ({
    getGrades: builder.query<GradeResponse, {
      teamId: number,
      date: string | null,
      onlyUnresearched: boolean,
      onlyNegative: boolean
    }>({
      query: ({teamId, date, onlyUnresearched, onlyNegative}) =>
        `?teamId=${teamId}&date=${date}&onlyUnresearched=${onlyUnresearched}&onlyNegative=${onlyNegative}`,
      providesTags: ['grades']
    }),
    updateGrade: builder.mutation({
      query: (grade: IGrade) => ({
        url: 'Grades',
        method: 'POST',
        body: grade
      }),
      invalidatesTags: ['grades']
    })
  }),
})

export const {
  useGetGradesQuery,
  useUpdateGradeMutation
} = gradeApi