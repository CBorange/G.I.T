
CREATE TABLE "analysis_route"
(
  "id"                   smallint     NOT NULL GENERATED ALWAYS AS IDENTITY,
  "source_provider_id"   smallint    ,
  "source_category_id"   smallint    ,
  "analyzer_provider_id" smallint     NOT NULL,
  "prompt_policy_code"   varchar(100) NOT NULL,
  "is_enabled"           boolean      NOT NULL,
  "created_at"           timestamptz  NOT NULL,
  "updated_at"           timestamptz ,
  "is_default"           boolean      NOT NULL DEFAULT false,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "analysis_route" IS '분석기 라우팅 테이블';

COMMENT ON COLUMN "analysis_route"."id" IS 'ID';

COMMENT ON COLUMN "analysis_route"."source_provider_id" IS '대상 크롤링 소스 전략 ID';

COMMENT ON COLUMN "analysis_route"."source_category_id" IS '대상 카테고리 ID';

COMMENT ON COLUMN "analysis_route"."analyzer_provider_id" IS 'AI 분석기 전략 ID';

COMMENT ON COLUMN "analysis_route"."prompt_policy_code" IS '분석 프롬프트 종류 코드';

COMMENT ON COLUMN "analysis_route"."is_enabled" IS '라우팅 정책 활성화 여부';

COMMENT ON COLUMN "analysis_route"."created_at" IS '생성일';

COMMENT ON COLUMN "analysis_route"."updated_at" IS '수정일';

COMMENT ON COLUMN "analysis_route"."is_default" IS '기본 라우터';

CREATE TABLE "analyze_job"
(
  "id"                   uuid         NOT NULL,
  "raw_contents_id"      uuid         NOT NULL,
  "analyzer_provider_id" smallint     NOT NULL,
  "prompt_policy_code"   varchar(100) NOT NULL,
  "status"               varchar(20)  NOT NULL,
  "attempt_count"        smallint     NOT NULL,
  "max_atempt_count"     smallint    ,
  "last_error"           text        ,
  "last_running_at"      timestamptz ,
  "ended_at"             timestamptz ,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "analyze_job" IS '분석 작업';

COMMENT ON COLUMN "analyze_job"."id" IS '기본키';

COMMENT ON COLUMN "analyze_job"."raw_contents_id" IS '작업 대상 원문 ID';

COMMENT ON COLUMN "analyze_job"."analyzer_provider_id" IS '분석 시점의 분석기 ID';

COMMENT ON COLUMN "analyze_job"."prompt_policy_code" IS '분석 프롬프트 종류 코드(스냅샷)';

COMMENT ON COLUMN "analyze_job"."status" IS '상태';

COMMENT ON COLUMN "analyze_job"."attempt_count" IS '분석 시도 횟수';

COMMENT ON COLUMN "analyze_job"."max_atempt_count" IS '최대 재시도 횟수';

COMMENT ON COLUMN "analyze_job"."last_error" IS '마지막 실패 사유';

COMMENT ON COLUMN "analyze_job"."last_running_at" IS '마지막 시도 일시';

COMMENT ON COLUMN "analyze_job"."ended_at" IS '최종 성공/실패 시각';

CREATE TABLE "analyzed_contents"
(
  "id"                    uuid         NOT NULL,
  "analyzer_provider_id"  smallint     NOT NULL,
  "raw_contents_id"       uuid         NOT NULL,
  "analyze_job_id"        uuid         NOT NULL,
  "actual_category_id"    smallint     NOT NULL,
  "title_summary"         text         NOT NULL,
  "body_summary"          text         NOT NULL,
  "keyword_json"          jsonb       ,
  "location_json"         jsonb       ,
  "model_name"            varchar(100) NOT NULL,
  "analysis_payload_json" jsonb       ,
  "analyzed_at"           timestamptz  NOT NULL,
  "created_at"            timestamptz  NOT NULL,
  "updated_at"            timestamptz ,
  "confidence"            numeric(5,4) NOT NULL,
  "confidence_reason"     text         NOT NULL,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "analyzed_contents" IS 'AI 분석 데이터(2차 가공)';

COMMENT ON COLUMN "analyzed_contents"."id" IS '기본키';

COMMENT ON COLUMN "analyzed_contents"."analyzer_provider_id" IS '제공자 ID';

COMMENT ON COLUMN "analyzed_contents"."raw_contents_id" IS '원문 데이터 ID';

COMMENT ON COLUMN "analyzed_contents"."analyze_job_id" IS '분석 작업 ID';

COMMENT ON COLUMN "analyzed_contents"."actual_category_id" IS '실제 카테고리 ID';

COMMENT ON COLUMN "analyzed_contents"."title_summary" IS '타이틀 분석 결과';

COMMENT ON COLUMN "analyzed_contents"."body_summary" IS '내용 분석 결과';

COMMENT ON COLUMN "analyzed_contents"."keyword_json" IS 'AI가 추출한 키워드 목록과 부가정보';

COMMENT ON COLUMN "analyzed_contents"."location_json" IS 'AI가 본문에서 추출한 지역 정보 목록';

COMMENT ON COLUMN "analyzed_contents"."model_name" IS '모델명(스냅샷)';

COMMENT ON COLUMN "analyzed_contents"."analysis_payload_json" IS 'AI 분석 실행 당시의 요청/응답/판단 근거 메타데이터';

COMMENT ON COLUMN "analyzed_contents"."analyzed_at" IS '분석일';

COMMENT ON COLUMN "analyzed_contents"."created_at" IS '생성일';

COMMENT ON COLUMN "analyzed_contents"."updated_at" IS '수정일(재분석 및 갱신일)';

COMMENT ON COLUMN "analyzed_contents"."confidence" IS '분석 신뢰도';

COMMENT ON COLUMN "analyzed_contents"."confidence_reason" IS '신뢰도 채점 근거';

CREATE TABLE "analyzer_provider"
(
  "id"              smallint     NOT NULL GENERATED ALWAYS AS IDENTITY,
  "name"            varchar(100) NOT NULL,
  "code"            varchar(50)  NOT NULL UNIQUE,
  "model_name"      varchar(100) NOT NULL,
  "endpoint_url"    text        ,
  "is_enabled"      boolean      NOT NULL,
  "config_json"     jsonb       ,
  "created_at"      timestamptz  NOT NULL,
  "updated_at"      timestamptz ,
  "last_running_at" timestamptz ,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "analyzer_provider" IS 'AI 분석기 전략';

COMMENT ON COLUMN "analyzer_provider"."id" IS 'ID';

COMMENT ON COLUMN "analyzer_provider"."name" IS '분석기명';

COMMENT ON COLUMN "analyzer_provider"."code" IS '코드';

COMMENT ON COLUMN "analyzer_provider"."model_name" IS 'AI 모델명(스냅샷)';

COMMENT ON COLUMN "analyzer_provider"."endpoint_url" IS 'API Endpoint(필요한 경우)';

COMMENT ON COLUMN "analyzer_provider"."is_enabled" IS '활성화 여부';

COMMENT ON COLUMN "analyzer_provider"."config_json" IS '모델 설정 JSON(필요한 경우)';

COMMENT ON COLUMN "analyzer_provider"."created_at" IS '생성일';

COMMENT ON COLUMN "analyzer_provider"."updated_at" IS '수정일';

COMMENT ON COLUMN "analyzer_provider"."last_running_at" IS '마지막 분석 일시';

CREATE TABLE "crawl_target"
(
  "id"                 int          NOT NULL GENERATED ALWAYS AS IDENTITY,
  "source_provider_id" smallint     NOT NULL,
  "source_category_id" smallint     NOT NULL,
  "name"               varchar(100) NOT NULL,
  "code"               varchar(50)  NOT NULL UNIQUE,
  "entry_url"          text         NOT NULL,
  "request_delay_ms"   integer     ,
  "is_active"          boolean      NOT NULL DEFAULT TRUE,
  "created_at"         timestamptz  NOT NULL,
  "updated_at"         timestamptz ,
  "last_running_at"    timestamptz ,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "crawl_target" IS '카테고리별 크롤링 대상 정보(소분류)';

COMMENT ON COLUMN "crawl_target"."id" IS 'ID';

COMMENT ON COLUMN "crawl_target"."source_provider_id" IS '크롤링 전략';

COMMENT ON COLUMN "crawl_target"."source_category_id" IS '기대 카테고리';

COMMENT ON COLUMN "crawl_target"."name" IS '제공자명';

COMMENT ON COLUMN "crawl_target"."code" IS '세부 크롤링 대상 구현체 코드';

COMMENT ON COLUMN "crawl_target"."entry_url" IS '크롤링 시작 URL';

COMMENT ON COLUMN "crawl_target"."request_delay_ms" IS 'entry당 요청 대기 시간';

COMMENT ON COLUMN "crawl_target"."is_active" IS '활성화 여부';

COMMENT ON COLUMN "crawl_target"."created_at" IS '생성일';

COMMENT ON COLUMN "crawl_target"."updated_at" IS '수정일';

COMMENT ON COLUMN "crawl_target"."last_running_at" IS '마지막 실행 일시';

CREATE TABLE "raw_contents"
(
  "id"               uuid         NOT NULL,
  "crawl_target_id"  int          NOT NULL,
  "source_url"       text         NOT NULL UNIQUE,
  "content_id"       varchar(50)  UNIQUE,
  "author"           varchar(100),
  "published_at"     timestamptz ,
  "title"            text         NOT NULL,
  "body"             text        ,
  "raw_payload_json" jsonb       ,
  "crawled_at"       timestamptz  NOT NULL,
  "created_at"       timestamptz  NOT NULL,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "raw_contents" IS '크롤링 원문 데이터(1차 가공)';

COMMENT ON COLUMN "raw_contents"."id" IS '기본키';

COMMENT ON COLUMN "raw_contents"."crawl_target_id" IS '크롤링 대상 ID';

COMMENT ON COLUMN "raw_contents"."source_url" IS '원문 URL';

COMMENT ON COLUMN "raw_contents"."content_id" IS '소스 고유 ID(기사 ID)';

COMMENT ON COLUMN "raw_contents"."author" IS '데이터 제공자(언론사, 사이트명 등)';

COMMENT ON COLUMN "raw_contents"."published_at" IS '원문 게시일';

COMMENT ON COLUMN "raw_contents"."title" IS '제목';

COMMENT ON COLUMN "raw_contents"."body" IS '내용';

COMMENT ON COLUMN "raw_contents"."raw_payload_json" IS '크롤러가 수집한 원본 응답/파싱 메타데이터 JSON';

COMMENT ON COLUMN "raw_contents"."crawled_at" IS '크롤링 일시';

COMMENT ON COLUMN "raw_contents"."created_at" IS '생성일';

CREATE TABLE "source_category"
(
  "id"          smallint     NOT NULL GENERATED ALWAYS AS IDENTITY,
  "code"        varchar(50)  NOT NULL UNIQUE,
  "name"        varchar(100) NOT NULL,
  "description" varchar(200),
  "is_active"   boolean      NOT NULL DEFAULT TRUE,
  "created_at"  timestamptz  NOT NULL,
  "updated_at"  timestamptz ,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "source_category" IS '소스 데이터 카테고리(전략/분류용)';

COMMENT ON COLUMN "source_category"."id" IS 'ID';

COMMENT ON COLUMN "source_category"."code" IS '카테고리 코드';

COMMENT ON COLUMN "source_category"."name" IS '카테고리명';

COMMENT ON COLUMN "source_category"."description" IS '설명';

COMMENT ON COLUMN "source_category"."is_active" IS '활성화 여부';

COMMENT ON COLUMN "source_category"."created_at" IS '생성일';

COMMENT ON COLUMN "source_category"."updated_at" IS '수정일';

CREATE TABLE "source_provider"
(
  "id"               smallint     NOT NULL GENERATED ALWAYS AS IDENTITY,
  "name"             varchar(100) NOT NULL,
  "code"             varchar(50)  NOT NULL UNIQUE,
  "base_url"         text         NOT NULL,
  "is_active"        boolean      NOT NULL DEFAULT TRUE,
  "request_delay_ms" integer      NOT NULL,
  "description"      varchar(200),
  "created_at"       timestamptz  NOT NULL,
  "updated_at"       timestamptz ,
  "last_running_at"  timestamptz ,
  PRIMARY KEY ("id")
);

COMMENT ON TABLE "source_provider" IS '크롤링 소스 및 전략(대분류)';

COMMENT ON COLUMN "source_provider"."id" IS 'ID';

COMMENT ON COLUMN "source_provider"."name" IS '제공자명';

COMMENT ON COLUMN "source_provider"."code" IS '크롤러 구현체 코드';

COMMENT ON COLUMN "source_provider"."base_url" IS '사이트 URL';

COMMENT ON COLUMN "source_provider"."is_active" IS '활성화 여부';

COMMENT ON COLUMN "source_provider"."request_delay_ms" IS 'entry당 요청 대기 시간(기본값)';

COMMENT ON COLUMN "source_provider"."description" IS '설명';

COMMENT ON COLUMN "source_provider"."created_at" IS '생성일';

COMMENT ON COLUMN "source_provider"."updated_at" IS '수정일';

COMMENT ON COLUMN "source_provider"."last_running_at" IS '마지막 실행 일시';

ALTER TABLE "analysis_route"
  ADD CONSTRAINT "FK_analyzer_provider_TO_analysis_route"
    FOREIGN KEY ("analyzer_provider_id")
    REFERENCES "analyzer_provider" ("id");

ALTER TABLE "analyzed_contents"
  ADD CONSTRAINT "FK_analyzer_provider_TO_analyzed_contents"
    FOREIGN KEY ("analyzer_provider_id")
    REFERENCES "analyzer_provider" ("id");

ALTER TABLE "crawl_target"
  ADD CONSTRAINT "FK_source_provider_TO_crawl_target"
    FOREIGN KEY ("source_provider_id")
    REFERENCES "source_provider" ("id");

ALTER TABLE "raw_contents"
  ADD CONSTRAINT "FK_crawl_target_TO_raw_contents"
    FOREIGN KEY ("crawl_target_id")
    REFERENCES "crawl_target" ("id");

ALTER TABLE "analysis_route"
  ADD CONSTRAINT "FK_source_provider_TO_analysis_route"
    FOREIGN KEY ("source_provider_id")
    REFERENCES "source_provider" ("id");

ALTER TABLE "analyzed_contents"
  ADD CONSTRAINT "FK_analyze_job_TO_analyzed_contents"
    FOREIGN KEY ("analyze_job_id")
    REFERENCES "analyze_job" ("id");

ALTER TABLE "analyze_job"
  ADD CONSTRAINT "FK_raw_contents_TO_analyze_job"
    FOREIGN KEY ("raw_contents_id")
    REFERENCES "raw_contents" ("id");

ALTER TABLE "analyze_job"
  ADD CONSTRAINT "FK_analyzer_provider_TO_analyze_job"
    FOREIGN KEY ("analyzer_provider_id")
    REFERENCES "analyzer_provider" ("id");

ALTER TABLE "analyzed_contents"
  ADD CONSTRAINT "FK_source_category_TO_analyzed_contents"
    FOREIGN KEY ("actual_category_id")
    REFERENCES "source_category" ("id");

ALTER TABLE "crawl_target"
  ADD CONSTRAINT "FK_source_category_TO_crawl_target"
    FOREIGN KEY ("source_category_id")
    REFERENCES "source_category" ("id");

ALTER TABLE "analysis_route"
  ADD CONSTRAINT "FK_source_category_TO_analysis_route"
    FOREIGN KEY ("source_category_id")
    REFERENCES "source_category" ("id");

ALTER TABLE "analyzed_contents"
  ADD CONSTRAINT "FK_raw_contents_TO_analyzed_contents"
    FOREIGN KEY ("raw_contents_id")
    REFERENCES "raw_contents" ("id");

CREATE UNIQUE INDEX "UQ_analysis_route_source_analyzer"
  ON "analysis_route" ("source_provider_id" ASC, "analyzer_provider_id" ASC);
