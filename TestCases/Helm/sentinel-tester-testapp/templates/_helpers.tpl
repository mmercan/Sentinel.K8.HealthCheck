{{/* vim: set filetype=mustache: */}}
{{/*
Expand the name of the chart.
*/}}
{{- define "sentinel-tester-testapp.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "sentinel-tester-testapp.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- if contains $name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "sentinel-tester-testapp.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Common labels
*/}}
{{- define "sentinel-tester-testapp.labels" -}}
helm.sh/chart: {{ include "sentinel-tester-testapp.chart" . }}
{{ include "sentinel-tester-testapp.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{/*
Selector labels
*/}}
{{- define "sentinel-tester-testapp.selectorLabels" -}}
app: {{ include "sentinel-tester-testapp.name" . }}
version: {{ .Chart.AppVersion  | quote }}
app.kubernetes.io/name: {{ include "sentinel-tester-testapp.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
branch:  {{ .Values.branch }}
{{- end -}}


{{- define "sentinel-tester-testapp.annotations" -}}
pipelines/run: {{ .Values.pipelines.run }}
pipelines/pipeline: {{ .Values.pipelines.pipeline }}
pipelines/jobName: {{ .Values.pipelines.job }}
pipelines/runuri: {{ .Values.pipelines.runuri | replace " " "%20" | replace "(" "%28" | replace ")" "%29" | replace "*" "%2A"}}
pipelines/project: {{ .Values.pipelines.project | replace " " "%20" | replace "(" "%28" | replace ")" "%29" | replace "*" "%2A"}}
pipelines/org: {{ .Values.pipelines.org }}
{{- end -}}

{{/*
Create the name of the service account to use
*/}}
{{- define "sentinel-tester-testapp.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
    {{ default (include "sentinel-tester-testapp.fullname" .) .Values.serviceAccount.name }}
{{- else -}}
    {{ default "default" .Values.serviceAccount.name }}
{{- end -}}
{{- end -}}
