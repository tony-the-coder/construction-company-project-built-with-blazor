-- ===== START SCRIPT =====

-- 1. Delete any row with a NULL PageKey
PRINT 'Step 1: Deleting row with NULL PageKey (if exists)...';
DELETE FROM [dbo].[PageContents]
WHERE [PageKey] IS NULL;
PRINT '-> Affected rows: ' + CAST(@@ROWCOUNT AS VARCHAR); -- Show how many rows were deleted

-- 2. Delete any row with a BLANK PageKey
PRINT 'Step 2: Deleting row with BLANK PageKey (if exists)...';
DELETE FROM [dbo].[PageContents]
WHERE [PageKey] = ''; -- Use = '' for empty string
PRINT '-> Affected rows: ' + CAST(@@ROWCOUNT AS VARCHAR); -- Show how many rows were deleted

-- 3. Delete existing 'AboutUsMain' just in case (to ensure clean insert)
PRINT 'Step 3: Deleting existing AboutUsMain row (if exists)...';
DELETE FROM [dbo].[PageContents]
WHERE [PageKey] = 'AboutUsMain';
PRINT '-> Affected rows: ' + CAST(@@ROWCOUNT AS VARCHAR); -- Show how many rows were deleted

-- 4. Insert the CORRECT row for 'AboutUsMain' with "20 years"
PRINT 'Step 4: Inserting correct AboutUsMain row...';
INSERT INTO [dbo].[PageContents] ([PageKey], [HtmlContent], [DateModified])
VALUES
(
    'AboutUsMain',  -- The CORRECT PageKey
    -- HTML Content with "20 years" and doubled single quotes ('')
    N'<p>Founded in Pickens, South Carolina, Lehman Custom Construction was born from a passion for building exceptional homes tailored to the unique visions of our clients. We believe that a custom home is more than just structure; it''s a personal sanctuary that should reflect your individual personality and lifestyle. Building your <strong class="font-semibold">dream home on your land</strong> is our specialty, and we are dedicated to transforming your chosen property into the perfect setting for your future.</p><p>With over <strong class="font-semibold">20 years of experience</strong> serving homeowners in the region, we''ve built a reputation for quality, integrity, and collaboration. Our approach is deeply rooted in <strong class="font-semibold">working closely with you</strong> every step of the way. We listen intently to your ideas, provide expert guidance, and maintain open communication throughout the entire design and construction process. We aim to make building your home a seamless and rewarding journey.</p><p>Our success is driven by our <strong class="font-semibold">team of dedicated professionals</strong> – skilled craftsmen, experienced project managers, and supportive staff who all share an unwavering commitment to excellence. We meticulously manage every detail, ensuring the highest standards of construction and finish work. This dedication is why a high percentage of our business comes from repeat customers and referrals – a testament to the trust and satisfaction we build with each project.</p><p>At Lehman Custom Construction, we don''t just build houses; we build lasting relationships and the homes where memories are made. We would be honored to be your trusted partner in bringing your vision to life.</p>',
    GETDATE() -- Use the current database server time
);
PRINT '-> Affected rows: ' + CAST(@@ROWCOUNT AS VARCHAR); -- Should be 1 if successful

-- 5. Verify the insertion - This SELECT should now return the row
PRINT 'Step 5: Verifying insertion...';
SELECT *
FROM [dbo].[PageContents]
WHERE [PageKey] = 'AboutUsMain';

-- ===== END SCRIPT =====